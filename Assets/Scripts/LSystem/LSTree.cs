using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LSTree : MonoBehaviour
{
    private const string axiom = "X";
    private string currentString = String.Empty;
    public bool refresh;
    private TransformInfo startTransform;
    [SerializeField] private int iterations = 4;
    [SerializeField] private GameObject branch;
    [SerializeField] private List<GameObject> branches = new List<GameObject>();
    [SerializeField] private float length = 10.0f;
    [SerializeField] private float angle = 30.0f;
    private Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
    private readonly Dictionary<char, string> rules = new Dictionary<char, string>
    {
        //{ 'X', "[FX][-FX][+FX]" },
        { 'X', "[F-[[X]+X]+F[+FX]-X]" },
        //{ 'X', "XFX-YF-YF+FX+FX-YF-YFFX+YF+FXFXYF-FX+YF+FXFX+YF-FXYF-YF-FX+FX+YFYF-" },
        //{ 'Y', "+FXFX-YF-YF+FX+FXYF+FX-YFYF-FX-YF+FXYFYF-FX-YFFX+FX+YF-YF-FX+FX+YFY" },
        { 'F', "FF" }
    };

    //private Dictionary<char, Action> functions;
    
    public struct TransformInfo
    {
        public TransformInfo(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
        public Vector3 position;
        public Quaternion rotation;
    }
    // private void Start()
    // {
    //     //GenerateTree();
    // }
    // private void Update()
    // {
    //     //if (refresh) { GenerateTree(); refresh = false; }
    // }
    public void GenerateTree()
    {
        Reset();
        
        for (int i = 0; i < iterations; i++)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in currentString) { sb.Append(rules.TryGetValue(c, out string foo) ? foo : c); }
            currentString = sb.ToString();
        }

        foreach (char c in currentString)
        {
            switch (c)
            {
                case 'F':
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
                    Vector3 initialPos = transform.position;
                    transform.Translate(Vector3.up * length);
                    GameObject treeSegment = Instantiate(branch, transform);
                    branches.Add(treeSegment);
                    LineRenderer treeSegmentLines = treeSegment.GetComponent<LineRenderer>();
                    treeSegmentLines.SetPosition(0, initialPos);
                    treeSegmentLines.SetPosition(1, transform.position);
                    break;
                }
                case 'X': { break; }
                case '+': { transform.Rotate(Vector3.back * angle); break; }
                case '-': { transform.Rotate(Vector3.forward * angle); break; }
                case '[': { transformStack.Push(new TransformInfo(transform.position, transform.rotation)); break; }
                case ']':
                {
                    TransformInfo ti = transformStack.Pop();
                    transform.position = ti.position;
                    transform.rotation = ti.rotation;
                    break;
                }
            }
        }
        transform.eulerAngles = new Vector3(0.0f, Game.instance.random.Next(0, 360), 0.0f);
    }
    private void Reset()
    {
        foreach (GameObject _branch in branches) { Destroy(_branch); }
        branches.Clear();
        transformStack.Clear();
        startTransform = new TransformInfo(transform.position, transform.rotation);
        currentString = axiom;
    }
}
