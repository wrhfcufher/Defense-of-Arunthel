using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttackSelectionController : AttackSelectionController
{
    List<TileController> selectableTiles = new List<TileController>();
    private List<TileController> aoeHoveredTiles = new List<TileController>();

    private int aoeRange;
    private TileController selectedTile;


    public AOEAttackSelectionController(int aoeRange)
    {
        this.aoeRange = aoeRange;
    }

    public override void OnTileMouseDown(int tileX, int tileY)
    {
        // only do the attack if a valid center is selected
        if (selectedTile == null)
        {
            return;
        }

        List<Unit> targets = new List<Unit>();

        foreach (TileController tc in aoeHoveredTiles)
        {
            if (tc.tile.unit != null && tc.tile.unit.team != sm.selectedUnit.unit.team)
            {
                targets.Add(tc.tile.unit);
            }
        }

        if (targets.Count != 0)
        {
            attack.useAttack(sm.selectedUnit.unit, targets.ToArray());
            attack.putOnCooldown();
            sm.removeInterruptingSelectionController(this);
        }
    }

    public override void onTileMouseEnter(TileController tc)
    {
        if (selectableTiles.Contains(tc)) // selected a valid center point
        {

            if (selectedTile == null)
            {
                // turn off indicators for possible centers
                foreach (TileController controller in selectableTiles)
                {
                    controller.setSelectionIndicatorState(false);
                }
            }

            // set as selected tile
            selectedTile = tc;


            // find range that this will hit
            HexBoard.PathingStep[] steps = sm.boardManager.getSelectableTiles(tc.x, tc.y, aoeRange);

            // add the tile controllers that will be part of the aoe
            foreach (HexBoard.PathingStep step in steps)
            {
                aoeHoveredTiles.Add(sm.boardManager.tileControllers[step.x, step.y]);
            }

            // turn on their indicators
            foreach (TileController controller in aoeHoveredTiles)
            {
                controller.setSelectionIndicatorState(true);
            }

        }
        else // tile not in aoe range was hovered, so show valid tiles that the aoe could be on
        {

            // deselect aoe hit tiles
            foreach (TileController controller in aoeHoveredTiles)
            {
                controller.setSelectionIndicatorState(false);
            }
            aoeHoveredTiles.Clear();

            // select valid aoe center tiles
            foreach(TileController controller in selectableTiles)
            {
                controller.setSelectionIndicatorState(true);
            }
        }
    }

    public override void onTileMouseExit(TileController tc)
    {
        if (selectedTile == tc)
        {
            selectedTile = null;

            // clear old aoe effect area
            foreach (TileController controller in aoeHoveredTiles)
            {
                controller.setSelectionIndicatorState(false);
            }
            aoeHoveredTiles.Clear();

            // select valid aoe center tiles
            foreach (TileController controller in selectableTiles)
            {
                controller.setSelectionIndicatorState(true);
            }
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
        foreach (TileController tc in aoeHoveredTiles)
        {
            tc.setSelectionIndicatorState(false);
        }

        foreach (TileController tc in selectableTiles)
        {
            tc.setSelectionIndicatorState(false);
        }
        selectableTiles.Clear();
        aoeHoveredTiles.Clear();
    }

    public override void onSelectionPaused()
    {
        onSelectionStopped();
    }

    public override void onSelectionResumed()
    {
        onSelectionStarted();
    }

    public override void onSelectionStarted()
    {
        HexBoard.PathingStep[] steps = sm.boardManager.getSelectableTiles(sm.selectedUnit.unit.x, sm.selectedUnit.unit.y, attack.attackRange);

        foreach (HexBoard.PathingStep step in steps)
        {
            selectableTiles.Add(sm.boardManager.tileControllers[step.x, step.y]);
        }

        foreach (TileController tc in selectableTiles)
        {
            tc.setSelectionIndicatorState(true);
        }
    }

    public override void onUnitListChanged()
    {
        // clear and reselect the tiles if a unit list change occured.
        onSelectionPaused();
        onSelectionStarted();
    }

    public override AttackSelectionController shallowCopy()
    {
        return new AOEAttackSelectionController(aoeRange);
    }
}
