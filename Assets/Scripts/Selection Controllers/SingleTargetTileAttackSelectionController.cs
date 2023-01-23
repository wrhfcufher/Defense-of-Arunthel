using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTargetTileAttackSelectionController : AttackSelectionController
{
    List<TileController> selectableTiles = new List<TileController>();

    public override void OnTileMouseDown(int tileX, int tileY)
    {
        foreach (TileController tc in selectableTiles)
        {
            if (tc.x == tileX && tc.y == tileY)
            {
                // use the attack on the target tile by making a fake unit that points to that tile
                attack.useAttack(sm.selectedUnit.unit, new Unit[] { new Unit(-1, UnitClass.getUnitClass(UnitClass.ClassName.Dummy), tileX, tileY) });
                attack.putOnCooldown();
                sm.removeInterruptingSelectionController(this);
                return;
            }
        }
    }

    public override void onTileMouseEnter(TileController tc)
    {

    }

    public override void onTileMouseExit(TileController tc)
    {

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
        foreach (TileController tc in selectableTiles)
        {
            tc.setSelectionIndicatorState(false);
        }
        selectableTiles.Clear();
    }

    public override void onSelectionPaused()
    {
        foreach (TileController tc in selectableTiles)
        {
            tc.setSelectionIndicatorState(false);
        }
        selectableTiles.Clear();
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
            Tile tile = sm.boardManager.getTile(step.x, step.y);
            if (tile.unit == null && tile.moveCost < 100)
            {
                selectableTiles.Add(sm.boardManager.tileControllers[step.x, step.y]);
            }
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
        return new SingleTargetTileAttackSelectionController();
    }
}
