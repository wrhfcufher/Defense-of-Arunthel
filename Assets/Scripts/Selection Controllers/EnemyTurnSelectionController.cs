using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyTurnSelectionController : SelectionController
{

    // used to keep track of the tile the user is mousing over
    private TileController mousedOverTile;

    public override void OnTileMouseDown(int tileX, int tileY)
    {
        
    }

    public override void onTileMouseEnter(TileController tc)
    {
        mousedOverTile = tc;
    }

    public override void onTileMouseExit(TileController tc)
    {
        if (mousedOverTile == tc)
        {
            mousedOverTile = null;
        }
    }

    public override void OnTileMouseUp(int tileX, int tileY)
    {

    }

    public override void OnUnitMouseDown(UnitController uc)
    {

    }

    public override void OnUnitMouseUp(UnitController uc)
    {
    }

    public override void onSelectionStopped()
    {
    }

    public override void onSelectionPaused()
    {
    }

    public override void onSelectionResumed()
    {
    }

    public override void onSelectionStarted()
    {

    }

    public override void onUnitListChanged()
    {
    }
}
