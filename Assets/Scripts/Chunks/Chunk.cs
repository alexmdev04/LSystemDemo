using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Chunk
{
    public Chunk(int _x, int _z, int _vertexCount, int _treeCount)
    {
        gridIndex = new int[2] { _x, _z };
        meshVertices = new Vector3[_vertexCount];
        treeCount = _treeCount;
    }
    public Chunk(int[] _gridIndex, int _vertexCount, int _treeCount)
    {
        gridIndex = _gridIndex;
        meshVertices = new Vector3[_vertexCount];
        treeCount = _treeCount;
    }
    public int[] gridIndex = null;
    public LoadedChunk loadedChunk = null;
    public Vector3[] meshVertices;
    public int treeCount;
    public List<Vector3> treePositions = new List<Vector3>();
    
    // public void Reset()
    // {
    //     gridIndex = null;
    //     loadedChunk = null;
    // }
}