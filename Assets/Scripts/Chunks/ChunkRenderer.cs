using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[RequireComponent(typeof(WorldGen))]
public class ChunkRenderer : MonoBehaviour
{
    public static ChunkRenderer instance { get; private set; }
    public bool 
        refresh,
        initialRenderComplete;
    public int 
        extraChecks = 0,
        renderDistance = 3;

    public List<LoadedChunk> 
        loadedMazePieces = new();

    public List<LoadedChunk> 
        chunkPool = new();
    Stack<LoadedChunk>
        chunkPoolAvailable;
    const string
        str_renderTime = "Chunk Renderer Update Time = ",
        str_ms = "ms",
        str_chunkPooled = "chunk (pooled)";
    void Awake()
    {
        instance = this;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7) | refresh)
        {
            refresh = false;
            MazeRenderUpdate();
        }
    }
    public void Reset()
    {
        for (int i = 0; i < loadedMazePieces.Count; i++) { ReturnToPool(loadedMazePieces[i]); }
        loadedMazePieces.Clear();
        initialRenderComplete = false;
        SetPoolSize(GetPoolSize());
    }
    public void MazeRenderUpdate()
    {      
        extraChecks = 0;
        System.Diagnostics.Stopwatch timer = new();
        timer.Start();
        // GETS ALL PIECES TO LOAD IN A DIAMOND SHAPE GRID AROUND THE PLAYER
        // THE RENDER DISTANCE DETERMINES HOW MANY PIECES AHEAD OF YOU WILL BE LOADED (NOT ACCOUNTING FOR PLAYER ROTATION)
        List<Chunk> mazePiecesToLoad = new();
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            int start = (((renderDistance * 2) + 1) - (Math.Abs(x) * 2) - 1) / 2;
            for (int z = start; z >= -start; z--)
            {
                if (z < -start) { break; }

                int[] chunkCheck = new int[2] { Player.instance.gridIndex[0] + x, Player.instance.gridIndex[1] + z };
                if (WorldGen.instance.TryGetMazePiece(chunkCheck, out Chunk mazePieceToLoad)) 
                { 
                    mazePiecesToLoad.Add(mazePieceToLoad);
                }
                else
                {
                    if (chunkCheck.IsOutOfBounds()) { continue; }
                    mazePiecesToLoad.Add(WorldGen.instance.GenerateNewChunkAt(chunkCheck));
                    //extraChecks++;
                }
            }
        }

        // FINDS PIECES TO BE UNLOADED AND RETURNS THEM TO THE POOL
        // ONLY CURRENTLY LOADED PIECES THAT ARE NOT PART OF mazePiecesToLoad SHOULD BE UNLOADED
        // THIS MAY LOOK UNNECESSARILY LONG BUT THERE IS NO OTHER WAY I SWEAR
        foreach (int[] mazePiecePosition in 
            loadedMazePieces.Select(loadedMazePiece => loadedMazePiece.chunk.gridIndex)
                            .Except(mazePiecesToLoad.Select(mazePiece => mazePiece.gridIndex))) 
        {
            ReturnToPool(WorldGen.instance.GridIndexToChunk(mazePiecePosition).loadedChunk);
        }

        loadedMazePieces.Clear();

        // LOADS MAZE PIECES, CAN INCLUDE ALREADY LOADED PIECES AS THEY WILL BE SKIPPED
        mazePiecesToLoad.ForEach(mazePiece => loadedMazePieces.Add(TakeFromPool(mazePiece)));
        
        timer.Stop();
    }
    public void SetRenderDistance(int renderDistanceNew)
    {
        renderDistance = renderDistanceNew;
        Reset();
        MazeRenderUpdate();
    }
    void SetPoolSize(int poolSize)
    {
        // ADJUSTS THE SIZE OF THE POOL TO THE EXACT AMOUNT TO FILL THE RENDER DISTANCE OR THE ENTIRE MAZE
        AdjustPool(poolSize - chunkPool.Count);
        chunkPoolAvailable = new(chunkPool);
    }
    void AdjustPool(int amount)
    {
        if (amount > 0) // EXPAND POOL
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject mazePieceNew = Instantiate(WorldGen.instance.mazePiecePrefab, gameObject.transform);
                // SceneManager.MoveGameObjectToScene(mazePieceNew, gameObject.scene);
                // mazePieceNew.transform.parent = gameObject.transform;
                LoadedChunk newLoadedChunk = mazePieceNew.GetComponent<LoadedChunk>();
                newLoadedChunk.Init();
                chunkPool.Add(newLoadedChunk);
            }
            Debug.Log("chunkPool +" + amount);
        }
        else if (amount < 0) // SHRINK POOL
        {
            Debug.Log("chunkPool " + amount);
            amount = Math.Abs(amount);
            for (int i = chunkPool.Count - amount; i < chunkPool.Count; i++)
            { Destroy(chunkPool[i].gameObject); }
            chunkPool.RemoveRange(chunkPool.Count - amount, amount);
        }
        else { return; }
    }
    void ResetPool()
    {
        // DESTROYS THE OLD POOL
        chunkPool.ForEach(mazePiece => Destroy(mazePiece.gameObject));
        chunkPool.Clear();
        chunkPoolAvailable.Clear();
    }
    LoadedChunk TakeFromPool(Chunk chunk)
    {
        // ASSIGNS A Chunk TO A LoadedChunk FROM THE POOL, TO BE VISIBLE IN THE WORLD
        if (chunk.loadedChunk is not null) { return chunk.loadedChunk; }
        if (!chunkPoolAvailable.TryPop(out LoadedChunk loadedMazePiece)) 
        {
            Debug.LogError("pool exceeded");
            return null;
        }
        chunk.loadedChunk = loadedMazePiece;
        loadedMazePiece.chunk = chunk;
        loadedMazePiece.Refresh();
        loadedMazePiece.gameObject.SetActive(true);
        return loadedMazePiece;
    }
    void ReturnToPool(LoadedChunk loadedChunk)
    {
        // RESETS THE LoadedChunk AND RETURNS IT TO THE POOL
        loadedChunk.gameObject.name = str_chunkPooled;
        loadedChunk.gameObject.SetActive(false);
        loadedChunk.chunk.loadedChunk = null;
        loadedChunk.chunk = null;
        chunkPoolAvailable.Push(loadedChunk);
    }  
    int GetPoolSize()
    {
        // GETS EXACTLY THE AMOUNT REQUIRED TO FILL THE RENDER DISTANCE WHILE NOT EXCEEDING THE MAZE SIZE
        int poolSize = 1;
        bool poolSizeExceededMazeSize = false;
        for (int i = 1; i <= renderDistance; i++) 
        { 
            poolSize += 4 * i;  
            poolSizeExceededMazeSize = poolSize > WorldGen.instance.chunkCount;
            if (poolSizeExceededMazeSize) { renderDistance = i + 1; break; }
        }
        return poolSizeExceededMazeSize ? WorldGen.instance.chunkCount : poolSize;
    }
}