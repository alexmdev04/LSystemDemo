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
        enemy;
    public int 
        worldSizeX = 10,
        worldSizeZ = 10,
        worldSizeXNew,
        worldSizeZNew,
        chunkSize = 10,
        chunkCount;
    public Chunk[]
        chunkGrid = new Chunk[0];
    public Chunk
        chunkDefault { get; private set; } = new();
    public GameObject
        floor,
        exitBeacon;
    public MeshRenderer 
        floorRenderer;
    int
        currentPathIndex;
    Chunk
        startChunk,
        exitChunk;
    List<int[]> 
        endPieces = new(); 
    Chunk[] 
        mazeCurrentPath;
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
    void Awake()
    {
        instance = this;
        floorRenderer = floor.GetComponent<MeshRenderer>();
        worldSizeXNew = worldSizeX;
        worldSizeZNew = worldSizeZ;
    }
    void Update()
    {
        if (exitChunk is not null) {
            if (Game.instance.papersCollected >= Game.instance.paperCount & !won & Player.instance.gridIndex.EqualTo(exitChunk.gridIndex)) { Win(); } }
    }
    public void Reset()
    {
        if (mazePiecePrefab == null) { Debug.LogError(str_prefabError); return; }
        ui.instance.uiFadeAlphaSet(1);
        exitBeacon.SetActive(false);
        Game.instance.Pause(false);
        Game.instance.Reset();
        ui.instance.gameOver.gameObject.SetActive(false);
        ui.instance.settings.gameObject.SetActive(false);

        won = false;
         
        WorldGenerate();

        Player.instance.TeleportInstant(startChunk.gridIndex.GridIndexToWorldPosition() + new Vector3(5f, 1.15f, 5f),
            new Vector3(0f, directions[0].ToVector().VectorNormalToCardinal().Euler(), 0f));
        Game.instance.inGame = true;
        Player.instance.PlayerFreeze(false);
        Player.instance.lookActive = true;
        Player.instance.moveActive = true;
        enemy.SetActive(true);
    }
    void WorldGenerate()
    {       
        System.Diagnostics.Stopwatch 
            allocationTimer = new(),
            algorithmTimer = new();

        allocationTimer.Start();
        MazeGridSet();
        allocationTimer.Stop();
        allocationTime = allocationTimer.Elapsed;

        SetFloor();

        algorithmTimer.Start();
        MazeAlgorithm();
        algorithmTimer.Stop();
        algorithmTime = algorithmTimer.Elapsed;

        allocationTimer.Stop();
        Debug.Log(new StringBuilder(str_mazeAllocationTime).Append(allocationTimer.Elapsed.TotalMilliseconds)
            .Append(str_mazeAlgorithmTime).Append(algorithmTimer.Elapsed.TotalMilliseconds)
            .Append(str_mazeGenTotalTime).Append((allocationTimer.Elapsed + allocationTimer.Elapsed).TotalMilliseconds).Append(str_ms).ToString());

        ChunkRenderer.instance.Reset();
        if (startChunk.gridIndex.EqualTo(Player.instance.gridIndex)) { ChunkRenderer.instance.MazeRenderUpdate(); }
    }
    void SetFloor()
    {
        // TRANSFORMING FLOOR OBJECT
        floor.transform.localScale = new Vector3(worldSizeX * chunkSize, 1f, worldSizeZ * chunkSize);
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2, 0.5f, floor.transform.localScale.z / 2);
        floorRenderer.material.mainTextureScale = new Vector2(worldSizeX, worldSizeZ);
    }
    void MazeGridSet()
    {
        worldSizeX = Math.Clamp(worldSizeXNew, 2, 10000);
        worldSizeZ = Math.Clamp(worldSizeZNew, 2, 10000);
        chunkCount = worldSizeX * worldSizeZ;
        int index = 0;
        if (chunkCount == chunkGrid.Length)
        { // IF THE AMOUNT NEEDED IS ALREADY INSTANTIATED JUST RESET THE VALUES
            for (int x = 0; x < worldSizeX; x++)
            {           
                for (int z = 0; z < worldSizeZ; z++)
                {
                    chunkGrid[index].Reset();
                    chunkGrid[index].gridIndex = new int[2]{ x, z };
                    index++;
                }
            }
        }
        else
        {

            if (worldSizeZ != worldSizeX) { worldSizeZ = worldSizeX; }

            for (int i = 0; i < chunkGrid.Length; i++) { chunkGrid[i] = null; }

            chunkGrid = new Chunk[chunkCount];
            // CREATES AN EMPTY GRID
            for (int x = 0; x < worldSizeX; x++)
            {           
                for (int z = 0; z < worldSizeZ; z++)
                {
                    // CREATES EMPTY Chunk CLASS ASSIGNS ITS GRID INDEX ADDS IT TO THE ARRAY
                    Chunk chunkNew = new() {
                        gridIndex = new int[2] { x, z } };
                    chunkGrid[index] = chunkNew;
                    index++;
                }
            }
        }
        // GETS EDGE PIECES AND GETS ADJACENT PIECES
        GetEdgePieces();
        GetAdjacentMazePieces();
    }
    void GetEdgePieces()
    {
        for (int x = 0; x < worldSizeX; x++)
        {
            GridIndexToMazePiece(x, 0).EdgeCheck();
            GridIndexToMazePiece(x, worldSizeZ - 1).EdgeCheck();
        }
        for (int z = 0; z < worldSizeZ; z++)
        {
            GridIndexToMazePiece(0, z).EdgeCheck();
            GridIndexToMazePiece(worldSizeX - 1, z).EdgeCheck();
        }
    }
    void GetAdjacentMazePieces()
    {
        for (int i = 0; i < chunkGrid.Length; i++)
        {
            if (TryGetMazePiece(chunkGrid[i].gridIndex[0] + directions[0][0], chunkGrid[i].gridIndex[1] + directions[0][1], out Chunk up))
            {
                chunkGrid[i].adjacentPieces[0] = up;
                up.adjacentPieces[1] = chunkGrid[i];
            }
            if (TryGetMazePiece(chunkGrid[i].gridIndex[0] + directions[3][0], chunkGrid[i].gridIndex[1] + directions[3][1], out Chunk right))
            {
                chunkGrid[i].adjacentPieces[3] = right;
                right.adjacentPieces[2] = chunkGrid[i];
            }
        }
    }
    void MazeAlgorithm()
    {
        mazeCurrentPath = new Chunk[chunkCount];
        // SETS THE START OF THE MAZE
        startChunk = chunkGrid[Game.instance.random.Next(chunkCount)];
        startChunk.passed = true;
        mazeCurrentPath[0] = startChunk;
        // STARTS THE ALGORITHM
        Chunk currentChunk = NextInPath(startChunk);

        int iterations = 0, iterationInfiniteLoop = chunkCount * 10;

        NextMazePiece:

        iterations++;
        if (iterations > iterationInfiniteLoop) { throw new Exception("Infinite Loop Detected @ NextMazePiece"); }

        // CHECK IF THE ALGORITHM HAS BACK TRACKED TO THE START    
        if (currentChunk != startChunk)
        {
            // NEXT PIECE IN PATH
            currentPathIndex++;
            mazeCurrentPath[currentPathIndex] = currentChunk;
            currentChunk = NextInPath(currentChunk);
            goto NextMazePiece;
        }

        // END OF MAZE GENERATION
        exitChunk = GridIndexToMazePiece(GetExitPiecePosition());
        exitChunk.passed = true;

        GetPaperPositions();
        //Debug.Log("New Maze Complete, Start @ " + startingPiece.gridIndex.ToStringBuilder() + ", Exit @ " + mazeExit.gridIndex.ToStringBuilder());
        return;
    }
    Chunk NextInPath(Chunk currentChunk)
    {
        int iterations = 0, iterationInfiniteLoop = chunkCount * 10;

        Backtrack:

        iterations++;
        if (iterations > iterationInfiniteLoop) { throw new Exception("Infinite Loop Detected @ Backtrack"); }

        // CHECK IF ANY AVAILABLE ADJACENT PIECES
        if (!currentChunk.TryGetRandomAdjacentPiece(out Chunk nextMazePiece, out int[] toDirection))
        {
            // RECURSIVE BACKTRACKING
            mazeCurrentPath[currentPathIndex] = null;
            currentPathIndex--;
            if (currentPathIndex <= 0) { return mazeCurrentPath[0]; }
            currentChunk = mazeCurrentPath[currentPathIndex];
            goto Backtrack;
        }

        // OPENS THE PATHWAY BETWEEN THE PIECES
        currentChunk.OpenDirection(toDirection);
        nextMazePiece.OpenDirection(-toDirection[0], -toDirection[1]);
        nextMazePiece.passed = true;
        return nextMazePiece;
    }
    void GetPaperPositions()
    {      
        Game.instance.GetPaperCount();
        int 
            desiredPaperCount = Game.instance.paperCount,
            currentPaperCount = 0;
        for (int i = 0; currentPaperCount < desiredPaperCount; i++)
        {
            Chunk randomChunk = chunkGrid[Game.instance.random.Next(chunkGrid.Length)];
            if (!randomChunk.hasPaper 
               & randomChunk.WallsActiveIsGrEqTo(1) 
               & !randomChunk.gridIndex.EqualTo(startChunk.gridIndex)
               & !randomChunk.gridIndex.EqualTo(exitChunk.gridIndex))
            {
                randomChunk.hasPaper = true;
                currentPaperCount++;
            }
        }
    }
    int[] GetExitPiecePosition()
    // this is the only list element left to optimise 
    // but i did not have the time
    {
        int minDiff = Mathf.CeilToInt(chunkCount * 0.75f);
        for (int i = 0; i < chunkGrid.Length; i++)
        {
            if (chunkGrid[i].WallsActiveIsEqTo()) { endPieces.Add(chunkGrid[i].gridIndex); }
        }

        List<int[]> endPiecesOutsideMinDiff = new(endPieces);
        NewEndPiece:
        for (int i = 0; i < endPiecesOutsideMinDiff.Count; i++)
        {
            if (Math.Abs(endPiecesOutsideMinDiff[i][0] - startChunk.gridIndex[0]) > minDiff 
              & Math.Abs(endPiecesOutsideMinDiff[i][1] - startChunk.gridIndex[1]) > minDiff)
            {
                endPiecesOutsideMinDiff.RemoveAt(i);
            }
        }
        
        if (minDiff <= 0) { Debug.LogWarning("no valid exit position"); return new int[2]{ 0, 0 }; }
        if (endPiecesOutsideMinDiff.Count == 0) { minDiff--; goto NewEndPiece; }
        return endPiecesOutsideMinDiff[Game.instance.random.Next(endPiecesOutsideMinDiff.Count)];
    }
    public void OpenExit()
    {
        uiMessage.instance.New("You've got all the pages!");
        uiMessage.instance.New("Look up for the exit beacon");
        uiMessage.instance.SetTimer(5);
        exitBeacon.transform.position = exitChunk.gridIndex.GridIndexToWorldPosition() + new Vector3(5f, 50f, 5f);
        exitBeacon.SetActive(true);
    }
    readonly string[] winMsgs = new string[] 
    { 
        "If I had a cookie I would not give it to you and pat you on the back instead.",
        "Now go again.",
        "If you win again you might just go insane!"
    };
    void Win()
    {
        won = true;
        ui.instance.gameOver.gameObject.SetActive(true);
    }
    public bool TryGetMazePiece(int[] gridIndex, out Chunk chunk)
    {
        chunk = null;
        // RETURNS NULL IF gridIndex IS OUTSIDE THE MAZE
        if (gridIndex[0] > worldSizeX - 1 | gridIndex[0] < 0 | gridIndex[1] > worldSizeZ - 1 | gridIndex[1] < 0) { return false; }
        chunk = GridIndexToMazePiece(gridIndex);
        return true;
    }
    public bool TryGetMazePiece(int gridIndexX, int gridIndexZ, out Chunk chunk)
    {
        chunk = null;
        // RETURNS NULL IF gridIndex IS OUTSIDE THE MAZE
        if (gridIndexX > worldSizeX - 1 | gridIndexX < 0 | gridIndexZ > worldSizeZ - 1 | gridIndexZ < 0) { return false; }
        chunk = GridIndexToMazePiece(gridIndexX, gridIndexZ);
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
    public Chunk GridIndexToMazePiece(int[] gridIndex) 
    {
        int index = ((gridIndex[0] * worldSizeX) + gridIndex[1]) - (worldSizeX > worldSizeZ ? gridIndex[0] : 0);
        if (index > chunkGrid.Length - 1 | index < 0) { return null; }
        return chunkGrid[index];
    }
    public Chunk GridIndexToMazePiece(int gridIndexX, int gridIndexZ) 
    {
        int index = ((gridIndexX * worldSizeX) + gridIndexZ) - (worldSizeX > worldSizeZ ? gridIndexX : 0);
        if (index > chunkGrid.Length - 1 | index < 0) { return null; }
        return chunkGrid[index];
    }
}