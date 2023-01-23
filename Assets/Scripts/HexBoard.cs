using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  A class that contains the hex board data structure.
///  It is not a monobehavior and will be used just to store
///  information about the board.
///  
/// It also contains pathing algorithms that can be used
/// to move units and determine the range of attacks.
/// </summary>
public class HexBoard
{
    public Tile[,] tiles;

    private List<Unit>[] units;

    public int teams { get { return units.Length; } }

    public static int distanceBetweenTiles(int xs, int ys, int xf, int yf)
    {
        int v1 = ys - yf + (xf - xs + (xf & 1) - (xs & 1)) / 2;
        int v2 = xs - xf;
        return (Math.Abs(v1) + Math.Abs(v1 + v2) + Math.Abs(v2)) / 2;
    }
    private event System.Action onUnitListChange;
    public HexBoard(LevelData levelData)
    {
        // initalize the tile array
        tiles = levelData.convertToTiles();

        // Initalize the units array
        units = new List<Unit>[levelData.teamData.Length];
        for (int i = 0; i < units.Length; i++)
        {
            units[i] = new List<Unit>();
        }
    }

    // function to add a unit to the board and put it on a tile if needed
    public void addUnit(Unit u)
    {
        // Make sure the unit is on a valid team
        if (u.team < 0 || u.team >= units.Length)
        {
            throw new System.InvalidOperationException($"Unit's team: {u.team} is not in the team range");
        }

        // if the unit is on a tile, make sure it can actully be on that tile
        if (u.x != -1 || u.y != -1)
        {
            if (u.x < 0 || u.x >= tiles.GetLength(0))
            {
                throw new System.InvalidOperationException($"Unit's x position: {u.x} is not in range of the tiles");
            }
            else if (u.y < 0 || u.y >= tiles.GetLength(1))
            {
                throw new System.InvalidOperationException($"Unit's y position: {u.y} is not in range of the tiles");
            }
            else if (tiles[u.x, u.y].unit != null)
            {
                throw new System.InvalidOperationException($"Unit can not be added to the board because it's tile ({u.x}-{u.y}) is already occupied.");
            }

            tiles[u.x, u.y].unit = u;
        }

        units[u.team].Add(u);
        u.addOnDeathListener(() => {removeUnit(u);}); // when the unit dies automaticly remove it

        if (onUnitListChange != null)
        {
            onUnitListChange();
        }

    }
    public void removeUnit(Unit u)
    {
        if (!isUnitKnown(u))
        {
            throw new System.InvalidOperationException("Can not remove a unit that is not managed by the board");
        }
        tiles[u.x, u.y].unit = null;
        units[u.team].Remove(u);

        if (onUnitListChange != null)
        {
            onUnitListChange();
        }
    }

       public List<Unit> getUnits(int team)
    {
        if (team < 0 || team >= units.Length)
        {
            return null;
        }

        return units[team];
    }

    // helper function to determine if a unit is registered by the board.
    private bool isUnitKnown(Unit unit)
    {
        // Make sure the unit is on a valid team
        if (unit.team < 0 || unit.team >= units.Length)
        {
            return false;
        }

        return units[unit.team].Contains(unit);
    }

    // function that allows for units to easily be moved
    public void moveUnit(Unit unit, int newX, int newY)
    {
        if (!isUnitKnown(unit))
        {
            throw new System.InvalidOperationException("Units tried to move before being registered with the board.");
        }

        if (newX < 0 || newY < 0 || newX >= tiles.GetLength(0) || newY >= tiles.GetLength(1))
        {
            throw new System.InvalidOperationException("Units tried to move to a tile that is not on the board.");
        }

        if (tiles[newX, newY].unit != null)
        {
            throw new System.InvalidOperationException("Unit tried to move into an occupied tile.");
        }

        if (unit.x != -1 || unit.y != -1)
        {
            Tile t = tiles[unit.x, unit.y];
            if (t.unit != unit)
            {
                throw new System.InvalidOperationException("Unit tried to move away from a tile it was not on.");
            }
            t.unit = null;
        }

        tiles[newX, newY].unit = unit;
        unit.x = newX;
        unit.y = newY;
    }

    // Function to give all adjecent tile indexes of the specified tile
    public (int, int)[] getAdjacentTileIndexes(int x, int y)
    {
        List<(int, int)> toReturn = new List<(int, int)>();
        if (x >= 0 && y >= 0 && x < tiles.GetLength(0) && y < tiles.GetLength(1))
        {
            // loop through the different possible adjacent tiles
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int x1 = x, y1 = y;
                    switch (i)
                    {
                        // top and bottom
                        case 0:
                            y1 = y + j * 2 - 1;
                            break;
                        // upper left and right
                        case 1:
                            x1 = x + j * 2 - 1;
                            y1 = y + (x + 1) % 2;
                            break;
                        // lower left and right
                        case 2:
                            x1 = x + j * 2 - 1;
                            y1 = y - (x % 2);
                            break;
                    }

                    // check to make sure this tile is actully on the board
                    if (x1 >= 0 && y1 >= 0 && x1 < tiles.GetLength(0) && y1 < tiles.GetLength(1))
                    {
                        toReturn.Add((x1, y1));
                    }
                }
            }
        }

        return toReturn.ToArray();
    }

    // overload that allows a unit to be given directly to getMoveableTiles
    public PathingStep[] getMoveableTiles(Unit u, bool excludeStart = true)
    {
        return getMoveableTiles(u.x, u.y, u.currentMovement, u.team, excludeStart);
    }

    // calls getMoveableTiles with movementCostOne which causes it to disregard movement cost
    // usefull for abilities that have a certain tile range
    public PathingStep[] getSelectableTiles(int startx, int starty, int range)
    {
        return getMoveableTiles(startx, starty, range, -1, false, true);
    }

    // function that returns where a unit can move with a given amount of movement
    // use unitTeam = -1 if you want to allow moving through all units
    public PathingStep[] getMoveableTiles(int startx, int starty, int movement, int unitTeam, bool excludeStart = true, bool movementCostOne = false)
    {
        if (startx < 0 || startx >= tiles.GetLength(0) || starty < 0 || starty >= tiles.GetLength(1))
        {
            return new PathingStep[] { };
        }

        bool[,] visited = new bool[tiles.GetLength(0), tiles.GetLength(1)];
        List<PathingStep> toExpand = new List<PathingStep>();
        List<PathingStep> reachable = new List<PathingStep>();
        toExpand.Add(new PathingStep(startx, starty, movement, null, !excludeStart));

        while (toExpand.Count != 0)
        {
            PathingStep prevStep = toExpand[0];
            toExpand.RemoveAt(0);

            if (visited[prevStep.x, prevStep.y])
            {
                // optimal path to this tile was already found, which shouldn't be possible if I removed things from the list correctly.
                // just in case keeping these here until the code gets tested more.
                Debug.LogError("on no that shouldnt be possible");
                continue;
            }

            visited[prevStep.x, prevStep.y] = true;
            reachable.Add(prevStep);

            foreach ((int adjecentX, int adjecentY) in getAdjacentTileIndexes(prevStep.x, prevStep.y))
            {
                // if the unit team was given, check to make sure an enemy isn't on the tile
                if (unitTeam != -1 && tiles[adjecentX, adjecentY].unit != null && tiles[adjecentX, adjecentY].unit.team != unitTeam)
                {
                    continue;
                }

                int movementAfterMove;
                if (movementCostOne)
                {
                    movementAfterMove = prevStep.m - 1;
                }
                else
                {
                    movementAfterMove = prevStep.m - tiles[adjecentX, adjecentY].moveCost;
                }
                if (movementAfterMove >= 0 && !visited[adjecentX, adjecentY])
                {
                    PathingStep newStep = new PathingStep(adjecentX, adjecentY, movementAfterMove, prevStep, tiles[adjecentX, adjecentY].unit == null);
                    // add newStep to toExpand while keeping the list sorted by movement decending
                    int insertAt = 0;
                    for (; insertAt < toExpand.Count; insertAt++)
                    {
                        if (toExpand[insertAt].x == newStep.x && toExpand[insertAt].y == newStep.y)
                        {
                            if (toExpand[insertAt].m < newStep.m)
                            {
                                // remove the other path since a more efficient way to get there has been found
                                toExpand.RemoveAt(insertAt);
                                insertAt--;
                            }
                            else
                            {
                                // no reason to use this path since a better once was already found
                                insertAt = -1;
                                break;
                            }
                        }
                        else if (toExpand[insertAt].m < newStep.m)
                        {
                            break;
                        }
                    }

                    // insert the path if needed
                    if (insertAt != -1)
                    {
                        toExpand.Insert(insertAt, newStep);
                    }
                }
            }

        }

        if (excludeStart)
        {
            reachable.RemoveAt(0);
        }

        return reachable.ToArray();
    }

    // Returns the nearest target on specified team
    public PathingStep findNearestTarget(Unit currUnit, int targetTeam, int passThroughTeam)
    {
        // 2D array of flags signifying whether a hex has been visited
        bool[,] visited = new bool[tiles.GetLength(0), tiles.GetLength(1)];
        // Queue to store what still needs to be looked at
        List<PathingStep> toExpand = new List<PathingStep>();

        PathingStep bestPath = null;

        // Add position of current unit to queue of squares to check (movement should theoretically be infinite) (this is not an endable position because it is already occupied by the current unit)
        toExpand.Add(new PathingStep(currUnit.x, currUnit.y, currUnit.currentMovement, null, false));

        // Loop until there are no further expansions
        while (toExpand.Count != 0)
        {
            // starts at a PathingStep added to the pending queue.
            PathingStep prevStep = toExpand[0];
            toExpand.RemoveAt(0);

            // update flag
            visited[prevStep.x, prevStep.y] = true;

            Tile currentTile = tiles[prevStep.x, prevStep.y];

            // Loops through every adjacent tile
            foreach ((int adjecentX, int adjecentY) in getAdjacentTileIndexes(prevStep.x, prevStep.y))
            {
                if (!visited[adjecentX, adjecentY])
                {
                    Tile nextTile = tiles[adjecentX, adjecentY];

                    // can be negative
                    int movementAfterMove = prevStep.m - nextTile.moveCost;


                    // creates a PathingStep to the current adjacent location
                    PathingStep newStep = new PathingStep(adjecentX, adjecentY, movementAfterMove, prevStep, nextTile.unit == null);

                    // check if there is a unit on this tile that has the correct team.
                    // if so we found the closest unit so return the path that gets to it
                    if (nextTile.unit != null && nextTile.unit.team == targetTeam)
                    {
                        // check if this path is endable if so return it
                        // also need to check that the prevStep isnt the first one
                        // if it is we are already next to the unit and don't need to move
                        if (prevStep.endable || prevStep.prev == null)
                        {
                            return newStep;
                        }
                        else
                        {
                            if (bestPath == null)
                            {
                                bestPath = newStep;
                            }
                            else
                            {
                                int myEndableCount = stepsAway(prevStep);
                                int bestEndableCount = stepsAway(bestPath.prev);

                                if (myEndableCount < bestEndableCount)
                                {
                                    bestPath = newStep;
                                }
                            }
                        }
                    }

                    // don't add the path to toExpand if it goes through a non pass through unit
                    if (passThroughTeam != -1 && nextTile.unit != null && nextTile.unit.team != passThroughTeam)
                    {
                        continue;
                    }

                    // add newStep to toExpand while keeping the list sorted by movement decending
                    int insertAt = 0;
                    for (; insertAt < toExpand.Count; insertAt++)
                    {
                        if (toExpand[insertAt].x == newStep.x && toExpand[insertAt].y == newStep.y)
                        {
                            if (toExpand[insertAt].m < newStep.m)
                            {
                                // remove the other path since a more efficient way to get there has been found
                                toExpand.RemoveAt(insertAt);
                                insertAt--;
                            }
                            else
                            {
                                // no reason to use this path since a better once was already found
                                insertAt = -1;
                                break;
                            }
                        }
                        else if (toExpand[insertAt].m < newStep.m)
                        {
                            break;
                        }
                    }

                    // insert the path if needed
                    if (insertAt != -1)
                    {
                        toExpand.Insert(insertAt, newStep);
                    }
                }
            }

        }

        // no endable path was found just return the best one
        return bestPath;
    }


    // helper function that returns the # of steps before an endable step or positive cost
    private int stepsAway(PathingStep step)
    {
        PathingStep cur = step;
        int count = 0;
        while (cur != null && !cur.endable && cur.m < 0)
        {
            count++;
            cur = cur.prev;
        }

        return count;
    }

    // Returns the closest tile that is targetDistance away from targetX and targetY
    public PathingStep findTileWithDistanceFromTile(Unit currUnit, int passThroughTeam, int targetDistance, int targetX, int targetY)
    {
        // 2D array of flags signifying whether a hex has been visited
        bool[,] visited = new bool[tiles.GetLength(0), tiles.GetLength(1)];
        // Queue to store what still needs to be looked at
        List<PathingStep> toExpand = new List<PathingStep>();

        // Add position of current unit to queue of tiles to expand
        toExpand.Add(new PathingStep(currUnit.x, currUnit.y, currUnit.currentMovement, null, false));

        // Loop until there are no further expansions
        while (toExpand.Count != 0)
        {
            // starts at a PathingStep added to the pending queue.
            PathingStep prevStep = toExpand[0];
            toExpand.RemoveAt(0);

            // update flag
            visited[prevStep.x, prevStep.y] = true;

            Tile currentTile = tiles[prevStep.x, prevStep.y];

            // check if the tile is a valid place for the unit to stop
            if (prevStep.endable)
            {
                // check if the tile is the correct distance away
                if (distanceBetweenTiles(targetX, targetY, prevStep.x, prevStep.y) == targetDistance)
                {
                    // return it if it is
                    return prevStep;
                }
            }

            // Loops through every adjacent tile
            foreach ((int adjecentX, int adjecentY) in getAdjacentTileIndexes(prevStep.x, prevStep.y))
            {
                if (!visited[adjecentX, adjecentY])
                {
                    Tile nextTile = tiles[adjecentX, adjecentY];

                    int movementAfterMove = prevStep.m - nextTile.moveCost;
                    
                    // don't include invalid movements
                    if (movementAfterMove < 0)
                    {
                        continue;
                    }

                    // creates a PathingStep to the current adjacent location
                    PathingStep newStep = new PathingStep(adjecentX, adjecentY, movementAfterMove, prevStep, nextTile.unit == null);

                    // don't add the path to toExpand if it goes through a non pass through unit
                    if (passThroughTeam != -1 && nextTile.unit != null && nextTile.unit.team != passThroughTeam)
                    {
                        continue;
                    }

                    // add newStep to toExpand while keeping the list sorted by movement decending
                    int insertAt = 0;
                    for (; insertAt < toExpand.Count; insertAt++)
                    {
                        if (toExpand[insertAt].x == newStep.x && toExpand[insertAt].y == newStep.y)
                        {
                            if (toExpand[insertAt].m < newStep.m)
                            {
                                // remove the other path since a more efficient way to get there has been found
                                toExpand.RemoveAt(insertAt);
                                insertAt--;
                            }
                            else
                            {
                                // no reason to use this path since a better once was already found
                                insertAt = -1;
                                break;
                            }
                        }
                        else if (toExpand[insertAt].m < newStep.m)
                        {
                            break;
                        }
                    }

                    // insert the path if needed
                    if (insertAt != -1)
                    {
                        toExpand.Insert(insertAt, newStep);
                    }
                }
            }

        }

        // no path to a tile with that distance was found, so return null
        return null;
    }

    public void addOnUnitListChangeListener(System.Action listener)
    {
        onUnitListChange += listener;
    }

    public void removeOnUnitListChangeListener(System.Action listener)
    {
        onUnitListChange -= listener;
    }

    // class used for the individual steps of the pathing algorithm
    public class PathingStep
    {
        public readonly int x, y, m;
        public readonly PathingStep prev;
        public readonly bool endable = false;
        public PathingStep(int x, int y, int m, PathingStep prev, bool endable)
        {
            this.x = x;
            this.y = y;
            this.m = m;
            this.prev = prev;
            this.endable = endable;
        }
    }
}
