using System.Text;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Serialization;

public class LoadedChunk : MonoBehaviour
{
    public Chunk 
        chunk;
    public GameObject
        wallFwd,
        wallBack,
        wallLeft,
        wallRight;
    public Collectable
        paper;
    const string 
        str_chunk = "chunk ";
    public void Refresh()
    {
        gameObject.transform.position = chunk.gridIndex.GridIndexToWorldPosition();
        name = new StringBuilder(str_chunk).Append(chunk.gridIndex.ToStringBuilder()).ToString();
        wallFwd.SetActive(chunk.walls[0]);
        wallBack.SetActive(chunk.walls[1]);
        wallLeft.SetActive(chunk.walls[2]);
        wallRight.SetActive(chunk.walls[3]);
        paper.gameObject.SetActive(chunk.hasPaper);
    }
}
