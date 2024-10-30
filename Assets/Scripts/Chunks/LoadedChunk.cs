using System;
using System.Collections.Generic;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Serialization;

public class LoadedChunk : MonoBehaviour
{
    public Chunk 
        chunk;
    // public Collectable
    //     paper;
    const string 
        str_chunk = "chunk ";
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public List<LSTree> trees;
    public void Init()
    {
        meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
        meshCollider = transform.GetChild(0).GetComponent<MeshCollider>();
        trees = new List<LSTree>();
        for (int i = 0; i < WorldGen.instance.chunkTreeCount; i++)
        {
            GameObject newTreeObject = Instantiate(WorldGen.instance.treeSpawnerPrefab, gameObject.transform);
            //newTreeObject.transform.position = transform.position + chunk.meshVertices[Game.instance.random.Next(121)];
            LSTree newTree = newTreeObject.GetComponent<LSTree>();
            newTree.GenerateTree();
            trees.Add(newTree);
        }
    }
    public void Refresh()
    {
        gameObject.transform.position = chunk.gridIndex.GridIndexToWorldPosition();
        name = new StringBuilder(str_chunk).Append(chunk.gridIndex.ToStringBuilder()).ToString();
        meshFilter.mesh.vertices = chunk.meshVertices;
        meshCollider.sharedMesh = meshFilter.mesh;
        for (int i = 0; i < trees.Count; i++) { trees[i].transform.localPosition = chunk.treePositions[i]; }
    }
}
