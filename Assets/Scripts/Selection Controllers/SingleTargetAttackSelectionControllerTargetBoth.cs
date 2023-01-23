using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This selection controller is identical to SingleTargetAttackSelectionController
 other than that it can target both teams */
public class SingleTargetAttackSelectionControllerTargetBoth : SingleTargetAttackSelectionController
{
    List<TileController> selectableTiles = new List<TileController>();

    public override void OnTileMouseDown(int tileX, int tileY)
    {

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
        foreach (TileController tc in selectableTiles)
        {
            if (tc.tile.unit == uc.unit)
            {
                attack.useAttack(sm.selectedUnit.unit, new Unit[] { uc.unit });
                attack.putOnCooldown();
                sm.removeInterruptingSelectionController(this);
                return;
            }
        }
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
            selectableTiles.Add(sm.boardManager.tileControllers[step.x, step.y]);
        }

        foreach (TileController tc in selectableTiles)
        {
            tc.setSelectionIndicatorState(true);
        }
    }

    public override void onUnitListChanged()
    {
        // if this happens during this selector then a unit was killed or created,
        // which likely means the attack controller will be removed, so don't do anything
        // in the future we can put some special effects here if we want
    }

    public override AttackSelectionController shallowCopy()
    {
        return new SingleTargetAttackSelectionControllerTargetBoth();
    }
}
