using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to hold information about tiles, such as height, movement, and other important data about the tile
/// </summary>
public class Tile
{

    public readonly int  height, moveCost;
    public Unit unit;

    public Tile(int height = 0, int moveCost = 1)
    {
        this.height = height;
        this.moveCost = moveCost;
    }

}
