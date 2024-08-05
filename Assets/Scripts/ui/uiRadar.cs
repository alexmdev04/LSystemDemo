using System;
using UnityEngine;

public class uiRadar : MonoBehaviour
{
    public uiRadarCamera Camera { get; private set; }
    [SerializeField] TMPro.TextMeshProUGUI distanceToGoal;
    void Awake()
    {
        Camera = GetComponentInChildren<uiRadarCamera>();
    }
    void Update()
    {
        //if (LevelLoader.instance.inLevel) { distanceToGoal.text = MathF.Round(Vector3.Distance(Player.instance.transform.position, LevelLoader.instance.levelCurrent.currentSection.goal.transform.position), 1).ToString("0.0") + "m"; }
    }
}