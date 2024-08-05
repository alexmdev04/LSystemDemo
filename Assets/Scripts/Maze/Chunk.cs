using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Chunk
{
    public bool
        passed = false,
        debug = false,
        hasPaper = false;
    public bool[] 
        walls = new bool[4]{ true, true, true, true };
    [NonSerialized] public Chunk[]
        adjacentPieces = new Chunk[4];
    //public string[] adjacentPieceIndexes = new string[4];
    public int[] 
        gridIndex = null;
    public LoadedChunk
        loadedChunk = null;
    // void RandomizeWalls(int amount = 3, bool edgeCheck = true)
    // {
    //    amount = System.Math.Clamp(amount, 0, 3);
    //    List<GameObject> randomWalls = new(walls);
    //    randomWalls.ForEach(wall => wall.SetActive(true));
    //    for (int i = 0; i < amount; i++)
    //    {
    //        int randomIndex = UnityEngine.Random.Range(0, randomWalls.Count);
    //        randomWalls[randomIndex].SetActive(false);
    //        randomWalls.RemoveAt(randomIndex);
    //    }
    //    if (edgeCheck) { EdgeCheck(); }
    // }
    public void Reset()
    {
        passed = false;
        debug = false;
        hasPaper = false;
        for (int i = 0; i < 4; i++) { walls[i] = true; adjacentPieces[i] = null; }
        gridIndex = null;
        loadedChunk = null;
    }
    public void EdgeCheck()
    {
        walls[0] |= gridIndex[1] == WorldGen.instance.worldSizeZ - 1;
        walls[1] |= gridIndex[1] == 0;
        walls[2] |= gridIndex[0] == 0;
        walls[3] |= gridIndex[0] == WorldGen.instance.worldSizeZ - 1;
    }
    public void OpenDirection(int[] direction)
    {
        if (direction == null) { return; }
        for (int i = 0; i < walls.Length; i++)
        {
            if (direction.EqualTo(WorldGen.directions[i]))
            {
                walls[i] = false;
                break;
            }
        }
    }
    public void OpenDirection(int directionX, int directionZ)
    {
        for (int i = 0; i < walls.Length; i++)
        {
            if (directionX == WorldGen.directions[i][0] & directionZ == WorldGen.directions[i][1])
            {
                walls[i] = false;
                break;
            }
        }
    }
    public bool TryGetRandomAdjacentPiece(out Chunk chunk, out int[] direction)
    {
        chunk = null;
        int randomInt;
        for (int i = 0; i < adjacentPieces.Length; i++)
        {
            randomInt = Game.instance.random.Next(adjacentPieces.Length);
            if (adjacentPieces[randomInt] == null) { continue; }
            if (adjacentPieces[randomInt].passed) { continue; }
            chunk = adjacentPieces[randomInt];
            direction = WorldGen.directions[randomInt];
            return true;
        }
        direction = null;
        return false;
    }
    public bool WallsActiveIsGrEqTo(int count = 3) 
    { 
        int wallsActive = 0;
        for (int i = 0; i < walls.Length; i++) { if (walls[i]) { wallsActive++; } }
        return wallsActive >= count;
    }
    public bool WallsActiveIsEqTo(int count = 3) 
    { 
        int wallsActive = 0;
        for (int i = 0; i < walls.Length; i++) { if (walls[i]) { wallsActive++; } }
        return wallsActive == count;
    }
}