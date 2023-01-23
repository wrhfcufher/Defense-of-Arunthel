using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class will control all enemy units to move/attack when the player ends their turn.
public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    public BoardManager boardManager;
    [SerializeField]
    private int enemyTeam;
    [SerializeField]
    private int targetTeam;
    private TurnManager turnManager;

    public void subscribeStartOfTurn(TurnManager tm)
    {
        turnManager = tm;
        tm.addStartOfTurnListener(onTurnStart);
    }

    //At end of player turn, move each enemy unit and attack if possible
    private void onTurnStart()
    {
        // only do things if it is the enemies turn
        if ((turnManager.turnCount) % boardManager.teams != enemyTeam)
        {
            return;
        }

        StartCoroutine(enemyTurnCoroutine());

    }

    protected virtual IEnumerator enemyTurnCoroutine()
    {
        // have a delay before the enemies do anything
        yield return new WaitForSeconds(0.55f);
        // Iterate through all enemies (easier to attack one by one, doing multiple at once may cause issues)
        Unit[] enemyUnits = boardManager.getUnits(enemyTeam).ToArray();


        foreach (Unit enemyUnit in enemyUnits)
        {
            // Returns a PathingStep to the nearest target on team 0
            HexBoard.PathingStep targetPos = boardManager.findNearestTarget(enemyUnit, targetTeam);
            // Location next to target
            HexBoard.PathingStep furthestPos = targetPos.prev;

            // When > 0, the target will have enough movement to go to tile specified by furthest
            // If the position is not endable then the unit can't end its pathing there
            while (furthestPos != null && (furthestPos.m < 0 || !furthestPos.endable))
            {
                furthestPos = furthestPos.prev;
            }

            // calculate the closest the unit can get to the enemy
            int shortestEnemyDistance = HexBoard.distanceBetweenTiles(targetPos.x, targetPos.y, enemyUnit.x, enemyUnit.y);
            // loop through the rest of the path (which is only the part the unit can actully reach this turn
            if (furthestPos != null)
            {
                HexBoard.PathingStep intermediateStep = furthestPos;
                while (intermediateStep != null) // don't count the tile if it is not a valid ending position
                {
                    if (intermediateStep.endable)
                    {
                        // calculate the distance and update the best distance if needed
                        int distance = HexBoard.distanceBetweenTiles(targetPos.x, targetPos.y, furthestPos.x, furthestPos.y);
                        if (distance <= shortestEnemyDistance)
                        {
                            shortestEnemyDistance = distance;
                        }
                    }

                    intermediateStep = intermediateStep.prev;
                }
            }


            // find all the attacks that the unit could use to hit this enemy
            List<Attack> useableAttacks = new List<Attack>();
            foreach (Attack attack in enemyUnit.attacks)
            {
                if (attack.currentCooldown <= 0 && attack.attackRange >= shortestEnemyDistance &&
                    (attack.selectionController.GetType() == typeof(SingleTargetAttackSelectionController)) || attack.selectionController.GetType() == typeof(SelfTargetAttackSelectionController))
                {
                    useableAttacks.Add(attack);
                }
            }


            HexBoard.PathingStep moveToStep = furthestPos;

            Attack selectedAttack = null;
            // if there is a useable attack select one and set the move to tile to a tile with the max range for that attack
            if (useableAttacks.Count > 0)
            {
                selectedAttack = useableAttacks[new System.Random().Next(useableAttacks.Count)];

                int maxDistance = int.MinValue;
                HexBoard.PathingStep tmpStep = furthestPos;

                // if this happens then furthestPos is null, but there is still a valid move,
                // so use the pathing step that has the unit instead of null (prevents to unit from moving back and forth)
                if (tmpStep == null)
                {
                    tmpStep = targetPos;
                    while (tmpStep != null && tmpStep.prev != null)
                    {
                        tmpStep = tmpStep.prev;
                    }
                }

                // find the tile the unit can move to that is the correct distance from the enemy
                while (tmpStep != null)
                {
                    int distance = HexBoard.distanceBetweenTiles(targetPos.x, targetPos.y, tmpStep.x, tmpStep.y);
                    if ((tmpStep.endable || tmpStep.prev == null) && distance <= selectedAttack.attackRange && distance >= maxDistance)
                    {
                        moveToStep = tmpStep;
                        maxDistance = distance;
                    }

                    tmpStep = tmpStep.prev;
                }


                // if the best tile is not equal to the attack range try to move to one that is equal
                if (maxDistance != selectedAttack.attackRange)
                {
                    HexBoard.PathingStep betterStep = boardManager.findTileWithDistanceFromTile(enemyUnit, enemyTeam, selectedAttack.attackRange, targetPos.x, targetPos.y);
                    // if a tile with the correct distance was found move to it instead
                    if (betterStep != null)
                    {
                        moveToStep = betterStep;
                    }
                }

            }
            // Find the unit controller for the current unit
            UnitController uc = boardManager.getUnitControllerForUnit(enemyUnit);

            // move to the move to tile
            // if the moveToStep is null then there is no where to go, so the unit should not move
            if (moveToStep != null)
            {
                // dont move if the moveToStep is the one the unit is on
                if (moveToStep.prev != null)
                {
                    uc.unit.currentMovement = moveToStep.m;
                    boardManager.moveUnit(uc, moveToStep);
                }
                yield return new WaitUntil(() => { return !uc.controllerBusy; });
            }

            // small delay before enemy attacks
            yield return new WaitForSeconds(0.15f);

            if (selectedAttack != null)
            {
                if (selectedAttack.selectionController.GetType() == typeof(SelfTargetAttackSelectionController))
                {
                    selectedAttack.useAttack(enemyUnit, new Unit[] {});
                    selectedAttack.putOnCooldown();
                }
                else
                {
                    HexBoard.PathingStep[] steps = boardManager.getSelectableTiles(enemyUnit.x, enemyUnit.y, selectedAttack.attackRange);
                    foreach (HexBoard.PathingStep step in steps)
                    {
                        Unit unitOnTile = boardManager.getTile(step.x, step.y).unit;
                        if (unitOnTile != null && unitOnTile.team == targetTeam)
                        {
                            selectedAttack.useAttack(enemyUnit, new Unit[] { unitOnTile });
                            selectedAttack.putOnCooldown();
                            break;
                        }
                    }
                }
                // wait for the enemy to finish the attack animation
                yield return new WaitUntil(() => { return !uc.controllerBusy; });

                // another small delay before the next unit moves
                yield return new WaitForSeconds(0.75f);
            }
        }

        // put a small delay before ending the enemy turn
        yield return new WaitForSeconds(0.25f);

        turnManager.endTurn();
    }
}
