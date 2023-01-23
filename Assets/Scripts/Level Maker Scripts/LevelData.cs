using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A scriptableobject that holds all the information needed to load a level.
/// </summary>

[CreateAssetMenu(fileName = "New Level", menuName = "ScriptableObjects/Level Data", order = 1)]
public class LevelData : ScriptableObject
{

    public TileData[] tileData; // Unity doesn't serialize 2d arrays, so we have to store it in a normal array instead
    public int width; // keep track of the width so we know when a new row of tiles starts

    public TeamData[] teamData;

    public int height { get { return tileData.Length / width;  } }

    public Tile[,] convertToTiles()
    {
        Tile[,] tiles = new Tile[width, tileData.Length / width];
        for (int i = 0; i < tileData.Length; i++)
        {
            tiles[i % width, i / width] = tileData[i].getTile();
        }

        return tiles;
    }

    // internal classes used to serialize unit information
    [System.Serializable]
    public class TeamData
    {
        public UnitData[] unitData;
        public SpawnPoint[] spawnPoints; // hold spawn points seperatly in case we want to later make the players units persist between levels
    }

    [System.Serializable]
    public class UnitData
    {
        public int health;
        public int movement;
        public UnitClass.ClassName className;

        public UnitData()
        {

        }

        public UnitData(int health, int movement, UnitClass.ClassName className)
        {
            this.health = health;
            this.movement = movement;
            this.className = className;
        }
    }

    [System.Serializable]
    public class SpawnPoint
    {

        public int x, y;

        public SpawnPoint()
        {

        }

        public SpawnPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

}
