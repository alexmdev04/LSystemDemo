using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using Unity.Jobs;
using UnityEngine.Serialization;

[RequireComponent(typeof(ChunkRenderer))]
public class WorldGen : MonoBehaviour
{
    public static WorldGen instance { get; private set; }
    public bool 
        refresh,
        mazeRenderAuto = true;
    public GameObject
        mazePiecePrefab,
        treeSpawnerPrefab;
    public int 
        worldSizeX = 10,
        worldSizeZ = 10,
        worldSizeXNew,
        worldSizeZNew,
        chunkSize = 10,
        chunkCount,
        chunkFloorVectorFrequency = 16,
        chunkTreeCount = 4;

    public float chunkFloorHeightScale = 10.0f;
    [NonSerialized] public Chunk[]
        chunkGrid = new Chunk[0];
    public Chunk
        chunkDefault { get; private set; } = new(0, 0, 121, 0);
    public GameObject
        floor,
        exitBeacon;
    public MeshRenderer 
        floorRenderer;
    public bool 
        resetOnWin, won;
    public TimeSpan allocationTime { get; private set; }
    public TimeSpan algorithmTime { get; private set; }
    public static readonly int[][] directions = new int[4][]
    {
        new int[2]{ 0, 1 },
        new int[2]{ 0,-1 },
        new int[2]{-1, 0 },
        new int[2]{ 1, 0 }
    };
    const string 
        str_mazeAllocationTime = "New Maze;  Allocation Time = ",
        str_mazeAlgorithmTime = "ms, Algorithm Time = ",
        str_prefabError = "Maze Piece Prefab not assigned, check Assets/Models/",
        str_mazeGenTotalTime = "ms, Total = ",
        str_ms = "ms";
    readonly Vector3[] defaultPlaneVertices = new Vector3[121]
        {
            new Vector3(5, 0, 5),   new Vector3(4, 0, 5),   new Vector3(3, 0, 5),   new Vector3(2, 0, 5),   new Vector3(1, 0, 5),  new Vector3(0, 0, 5), 
            new Vector3(-1, 0, 5),  new Vector3(-2, 0, 5),  new Vector3(-3, 0, 5),  new Vector3(-4, 0, 5),  new Vector3(-5, 0, 5),
            new Vector3(5, 0, 4),   new Vector3(4, 0, 4),   new Vector3(3, 0, 4),   new Vector3(2, 0, 4),   new Vector3(1, 0, 4),  new Vector3(0, 0, 4),
            new Vector3(-1, 0, 4),  new Vector3(-2, 0, 4),  new Vector3(-3, 0, 4),  new Vector3(-4, 0, 4),  new Vector3(-5, 0, 4),
            new Vector3(5, 0, 3),   new Vector3(4, 0, 3),   new Vector3(3, 0, 3),   new Vector3(2, 0, 3),   new Vector3(1, 0, 3),  new Vector3(0, 0, 3),
            new Vector3(-1, 0, 3),  new Vector3(-2, 0, 3),  new Vector3(-3, 0, 3),  new Vector3(-4, 0, 3),  new Vector3(-5, 0, 3),
            new Vector3(5, 0, 2),   new Vector3(4, 0, 2),   new Vector3(3, 0, 2),   new Vector3(2, 0, 2),   new Vector3(1, 0, 2),  new Vector3(0, 0, 2),
            new Vector3(-1, 0, 2),  new Vector3(-2, 0, 2),  new Vector3(-3, 0, 2),  new Vector3(-4, 0, 2),  new Vector3(-5, 0, 2),
            new Vector3(5, 0, 1),   new Vector3(4, 0, 1),   new Vector3(3, 0, 1),   new Vector3(2, 0, 1),   new Vector3(1, 0, 1),  new Vector3(0, 0, 1),
            new Vector3(-1, 0, 1),  new Vector3(-2, 0, 1),  new Vector3(-3, 0, 1),  new Vector3(-4, 0, 1),  new Vector3(-5, 0, 1),
            new Vector3(5, 0, 0),   new Vector3(4, 0, 0),   new Vector3(3, 0, 0),   new Vector3(2, 0, 0),   new Vector3(1, 0, 0),  new Vector3(0, 0, 0),
            new Vector3(-1, 0, 0),  new Vector3(-2, 0, 0),  new Vector3(-3, 0, 0),  new Vector3(-4, 0, 0),  new Vector3(-5, 0, 0),
            new Vector3(5, 0, -1),  new Vector3(4, 0, -1),  new Vector3(3, 0, -1),  new Vector3(2, 0, -1),  new Vector3(1, 0, -1), new Vector3(0, 0, -1),
            new Vector3(-1, 0, -1), new Vector3(-2, 0, -1), new Vector3(-3, 0, -1), new Vector3(-4, 0, -1), new Vector3(-5, 0, -1),
            new Vector3(5, 0, -2),  new Vector3(4, 0, -2),  new Vector3(3, 0, -2),  new Vector3(2, 0, -2),  new Vector3(1, 0, -2), new Vector3(0, 0, -2),
            new Vector3(-1, 0, -2), new Vector3(-2, 0, -2), new Vector3(-3, 0, -2), new Vector3(-4, 0, -2), new Vector3(-5, 0, -2),
            new Vector3(5, 0, -3),  new Vector3(4, 0, -3),  new Vector3(3, 0, -3),  new Vector3(2, 0, -3),  new Vector3(1, 0, -3), new Vector3(0, 0, -3), 
            new Vector3(-1, 0, -3), new Vector3(-2, 0, -3), new Vector3(-3, 0, -3), new Vector3(-4, 0, -3), new Vector3(-5, 0, -3),
            new Vector3(5, 0, -4),  new Vector3(4, 0, -4),  new Vector3(3, 0, -4),  new Vector3(2, 0, -4),  new Vector3(1, 0, -4), new Vector3(0, 0, -4),
            new Vector3(-1, 0, -4), new Vector3(-2, 0, -4), new Vector3(-3, 0, -4), new Vector3(-4, 0, -4), new Vector3(-5, 0, -4),
            new Vector3(5, 0, -5),  new Vector3(4, 0, -5),  new Vector3(3, 0, -5),  new Vector3(2, 0, -5),  new Vector3(1, 0, -5), new Vector3(0, 0, -5),
            new Vector3(-1, 0, -5), new Vector3(-2, 0, -5), new Vector3(-3, 0, -5), new Vector3(-4, 0, -5), new Vector3(-5, 0, -5), };
    void Awake()
    {
        instance = this;
        floorRenderer = floor.GetComponent<MeshRenderer>();
        worldSizeXNew = worldSizeX;
        worldSizeZNew = worldSizeZ;
    }
    void Update()
    {
        // if (exitChunk is not null) {
        //     if (Game.instance.papersCollected >= Game.instance.paperCount & !won & Player.instance.gridIndex.EqualTo(exitChunk.gridIndex)) { Win(); } }
    }
    public void Reset()
    {
        if (mazePiecePrefab == null) { Debug.LogError(str_prefabError); return; }
        ui.instance.uiFadeAlphaSet(1);
        //exitBeacon.SetActive(false);
        Game.instance.Pause(false);
        Game.instance.Reset();
        ui.instance.gameOver.gameObject.SetActive(false);
        ui.instance.settings.gameObject.SetActive(false);

        won = false;
         
        WorldGenerate();
        
        Game.instance.inGame = true;
        Player.instance.PlayerFreeze(false);
        Player.instance.lookActive = true;
        Player.instance.moveActive = true;
        //enemy.SetActive(true);
    }
    void WorldGenerate()
    {       
        ChunkGridSet();
        ChunkRenderer.instance.Reset();
        ChunkRenderer.instance.MazeRenderUpdate();
    }
    void SetFloor()
    {
        // TRANSFORMING FLOOR OBJECT
        floor.transform.localScale = new Vector3(worldSizeX * chunkSize / 10.0f, 1f, worldSizeZ * chunkSize / 10.0f);
        floor.transform.position = new Vector3(floor.transform.localScale.x * 10.0f / 2.0f, 0.5f, floor.transform.localScale.z * 10.0f / 2.0f);
        floorRenderer.material.mainTextureScale = new Vector2(worldSizeX, worldSizeZ);
    }
    void ChunkGridSet()
    {
        worldSizeX = Math.Clamp(worldSizeXNew, 2, 1024);
        worldSizeZ = Math.Clamp(worldSizeZNew, 2, 1024);
        chunkCount = worldSizeX * worldSizeZ;
        chunkGrid = new Chunk[chunkCount];
    }
    public Chunk GenerateNewChunkAt(int[] gridPosition)
    {
        int index = GridIndexToChunkIndex(gridPosition);
        chunkGrid[index] = new Chunk(gridPosition, defaultPlaneVertices.Length, chunkTreeCount);
        for (int i = 0; i < defaultPlaneVertices.Length; i++)
        {
            chunkGrid[index].meshVertices[i] = new Vector3(
                defaultPlaneVertices[i].x,
                Mathf.PerlinNoise(
                    ((float)(gridPosition[0] * 10) + defaultPlaneVertices[i].x) * 0.1f,
                    ((float)(gridPosition[1] * 10) + defaultPlaneVertices[i].z) * 0.1f),
                defaultPlaneVertices[i].z
                );
        }
        chunkGrid[index].treeCount = chunkTreeCount;
        for (int i = 0; i < chunkGrid[index].treeCount; i++)
        {
            chunkGrid[index].treePositions.Add(
                chunkGrid[index].meshVertices[Game.instance.random.Next(0, chunkGrid[index].meshVertices.Length)]);
        }
        return chunkGrid[index];
    }
    void Win()
    {
        won = true;
        ui.instance.gameOver.gameObject.SetActive(true);
    }
    public bool TryGetMazePiece(int[] gridIndex, out Chunk chunk)
    {
        chunk = null;
        // RETURNS NULL IF gridIndex IS OUTSIDE THE MAZE
        if (gridIndex.IsOutOfBounds()) { return false; }
        //if (gridIndex[0] > worldSizeX - 1 | gridIndex[0] < 0 | gridIndex[1] > worldSizeZ - 1 | gridIndex[1] < 0) { return false; }
        chunk = GridIndexToChunk(gridIndex);
        return chunk is not null;
    }
    public bool TryGetMazePiece(int gridIndexX, int gridIndexZ, out Chunk chunk)
    {
        chunk = null;
        // RETURNS NULL IF gridIndex IS OUTSIDE THE MAZE
        if (GridIndexExt.IsOutOfBounds(gridIndexX, gridIndexZ)) { return false; }
        //if (gridIndexX > worldSizeX - 1 | gridIndexX < 0 | gridIndexZ > worldSizeZ - 1 | gridIndexZ < 0) { return false; }
        chunk = GridIndexToChunk(gridIndexX, gridIndexZ);
        return true;
    }
    // A REVERSE INDEX EQUATION IF NECESSARY
    // public int[] MazePieceIndexToGridIndex(int mazePieceIndex)
    // { 
    //     int z = mazePieceIndex % worldSizeX;
    //     int a = mazePieceIndex - z;
    //     int x = a < 0 ? 0 : a / worldSizeX;
    //     return new int[2] { x, z };
    // }
    public Chunk GridIndexToChunk(int[] gridIndex)
    {
        int index = GridIndexToChunkIndex(gridIndex);
        if (index == -1) { return null; }
        return chunkGrid[index];
    }
    public Chunk GridIndexToChunk(int gridIndexX, int gridIndexZ)
    {
        int index = GridIndexToChunkIndex(gridIndexX, gridIndexZ);
        if (index == -1) { return null; }
        return chunkGrid[index];
    }
    public int GridIndexToChunkIndex(int[] gridIndex) 
    {
        int index = (gridIndex[0] * worldSizeX) + gridIndex[1];
        if (index > chunkGrid.Length - 1 | index < 0) { return -1; }
        return index;
    }
    public int GridIndexToChunkIndex(int gridIndexX, int gridIndexZ) 
    {
        int index = (gridIndexX * worldSizeX) + gridIndexZ;
        if (index > chunkGrid.Length - 1 | index < 0) { return -1; }
        return index;
    }
}