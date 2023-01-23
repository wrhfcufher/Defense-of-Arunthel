using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that handles interaction between the unity gameobjects and the data structure that represents the board
/// </summary>
public class BoardManager : MonoBehaviour
{

    public float HexHeight { get; private set; } = 1;
    public float HexWidth { get; private set; } = 1;
    public float HexEdgeWidth { get; private set; } = 0.25f;
    public int teams { get; private set; } = 2;

    private HexBoard board;
    private TurnManager turnManager;
    public TileController[,] tileControllers;

    [HideInInspector]
    private List<UnitController> unitControllers;

    private bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void initBoard(HexBoard board, TileController[,] tileControllers, float hexHeight, float hexWidth, float edgeWidth)
    {
        if (initialized)
        {
            throw new System.InvalidOperationException("Tried to initalize a boardmanager that was already initialized");
        }

        this.board = board;
        this.tileControllers = tileControllers;
        unitControllers = new List<UnitController>();
        HexHeight = hexHeight;
        HexWidth = hexWidth;
        HexEdgeWidth = edgeWidth;
        teams = board.teams;
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Tile getTile(int x, int y)
    {
        return board.tiles[x, y];
    }

    public List<Unit> getUnits(int team){
        return board.getUnits(team);
    }

    public void addUnitController(UnitController uc){
        unitControllers.Add(uc);
    }
    public HexBoard.PathingStep findNearestTarget(Unit currUnit, int team){
        return board.findNearestTarget(currUnit, team, currUnit.team);
    }

    // Move unit along a path and play the unit conrollers movement animation
    public void moveUnit(UnitController uc, HexBoard.PathingStep step)
    {
        board.moveUnit(uc.unit, step.x, step.y);
        uc.moveToLocationAlongPath(getMovementPath(step, false));
    }

    // Teleport a unit to a specific tile without playing any animation
    public void teleportUnit(UnitController uc, int tileX, int tileY)
    {
        board.moveUnit(uc.unit, tileX, tileY);
        uc.transform.localPosition = tilePosToWorld(tileX, tileY);
    }

    public Vector3[] getMovementPath(HexBoard.PathingStep step, bool reversed = true)
    {
        HexBoard.PathingStep cur = step;

        List<Vector3> path = new List<Vector3>();
        HexBoard.PathingStep prev = null; // kinda of a confusing name since it is not equal to cur.prev, but I cant think of a better one
        while (cur != null)
        {
            Vector3 curLocation = tilePosToWorld(cur.x, cur.y);
            // If this is not the first path add one on the edge of the tile to reduce clipping
            if (prev != null)
            {
                Vector3 prevLocation = tilePosToWorld(prev.x, prev.y);
                Vector3 middleLocation = (curLocation + prevLocation) / 2;
                middleLocation.y = Mathf.Max(prevLocation.y, curLocation.y) + 0.08f;
                path.Add(middleLocation);
            }

            path.Add(curLocation + new Vector3(0, 0.08f, 0));
            prev = cur;
            cur = cur.prev;
        }

        if (!reversed)
        {
            path.Reverse();
        }

        return path.ToArray();
    }

    public Vector3 tilePosToWorld(int x, int y)
    {
        return tilePosToWorld(x, y, HexWidth, HexEdgeWidth, HexHeight, board);
    }


    public void subscribeEndOfTurn(TurnManager tm)
    {
        turnManager = tm;
        tm.addEndOfTurnListener(onTurnEnd);
    }

    private void onTurnEnd()
    {
        if ((turnManager.turnCount + 1) % teams == 0)
        {
            for (int i = 0; i < teams; i++)
            {
                foreach (Unit unit in board.getUnits(i))
                {
                    unit.reduceCooldowns();
                }
            }
        }
    }

    public void addOnUnitListChangeListener(System.Action listener)
    {
        board.addOnUnitListChangeListener(listener);
    }

    public void removeOnUnitListChangeListener(System.Action listener)
    {
        board.addOnUnitListChangeListener(listener);
    }

    // Method that will be called when the turn ends through an event
    public HexBoard.PathingStep[] getMoveableTiles(UnitController uc)
    {
        return board.getMoveableTiles(uc.unit);
    }

    public HexBoard.PathingStep[] getSelectableTiles(int startx, int starty, int range)
    {
        return board.getSelectableTiles(startx, starty, range);
    }

    public HexBoard.PathingStep findTileWithDistanceFromTile(Unit currUnit, int passThroughTeam, int targetDistance, int targetX, int targetY)
    {
        return board.findTileWithDistanceFromTile(currUnit, passThroughTeam, targetDistance, targetX, targetY);
    }

    public UnitController getUnitControllerForUnit(Unit unit)
    {
        foreach (UnitController uc in unitControllers)
        {
            if (uc.unit == unit)
            {
                return uc;
            }
        }

        return null;
    }


    // Static Methods
    // static version of tilePosToWorld that can be used without an initalized hexboard
    public static Vector3 tilePosToWorld(int x, int y, float HexWidth, float HexEdgeWidth, float HexHeight, HexBoard board)
    {
        return new Vector3(x * (HexWidth - HexEdgeWidth) + HexWidth / 2, 1 + board.tiles[x, y].height / 10f, y * HexHeight + HexHeight / 2 - HexHeight / 2 * (x % 2));
    }

}
