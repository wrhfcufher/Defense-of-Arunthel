using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfTargetAttackSelectionController : AttackSelectionController
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

    }

    public override void onSelectionStopped()
    {
    }

    public override void onSelectionPaused()
    {
    }

    public override void onSelectionResumed()
    {
        onSelectionStarted();
    }

    public override void onSelectionStarted()
    {
        attack.useAttack(sm.selectedUnit.unit, null);
        attack.putOnCooldown();
        sm.removeInterruptingSelectionController(this);
        HexBoard.PathingStep[] steps = sm.boardManager.getSelectableTiles(sm.selectedUnit.unit.x, sm.selectedUnit.unit.y, attack.attackRange);
    }

    public override void onUnitListChanged()
    {
        // if this happens during this selector then a unit was killed or created,
        // which likely means the attack controller will be removed, so don't do anything
        // in the future we can put some special effects here if we want
    }

    public override AttackSelectionController shallowCopy()
    {
        return new SelfTargetAttackSelectionController();
    }
}
