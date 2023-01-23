using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Tile data class that is used to keep track of tile data in the level data scriptable object.
/// </summary>

[System.Serializable]
public class TileData
{

    public int height;
    public int moveCost;
    public TileTexture texture;

    public TileData(int height, int moveCost)
    {
        this.height = height;
        this.moveCost = moveCost;
        texture = TileTexture.Basic;
    }

    public Tile getTile()
    {
        return new Tile(height, moveCost);
    }

    public enum TileTexture
    {
        Basic, Grass, Water, Brick, Dark_Stone, Wood, Cobblestone, Lava
    }
}
