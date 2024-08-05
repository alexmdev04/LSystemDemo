using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("Spinning values")]
    [SerializeField] bool spin = true;
    [SerializeField] float spinSpeed = 100f;
    [SerializeField] Vector3 spinDirection = Vector3.up;
    [Header("Bobbing values")]
    [SerializeField] bool bob = true;
    [SerializeField] float bobSpeed = 5f;
    [SerializeField] float bobAmplitude = 1f;
    [Space] 
    [SerializeField] TMPro.TextMeshPro count;
    public LoadedChunk loadedChunk;
    float startYPosition;

    // Checks if this object collides with the player
    void OnTriggerEnter(Collider collision) 
    { 
        if (collision.transform.parent.TryGetComponent(out Player player)) 
        { 
            uiDebugConsole.instance.InternalCommandCall("collectpage");
            loadedChunk.chunk.hasPaper = false;
            gameObject.SetActive(false);
        }
    }
    
    void Start() { startYPosition = transform.position.y; } // Sets the start position for use with bobbing
    
    void Update() // Iterates on the position and rotation of the object to give a bobbing and spinning effect
    {
        if (bob) { transform.position = new Vector3 (transform.position.x, startYPosition + (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude), transform.position.z); }
        if (spin) { transform.Rotate(spinDirection, spinSpeed * Time.deltaTime, Space.Self); }
        count.text = (Game.instance.papersCollected + 1).ToString();
    }
}