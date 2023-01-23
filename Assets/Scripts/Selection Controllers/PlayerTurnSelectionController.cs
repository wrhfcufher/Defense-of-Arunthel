using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerTurnSelectionController : SelectionController
{
    public int playerTeam;

    // values used to keep track of the selected unit and where it can move
    
    private HexBoard.PathingStep[] steps;

    private List<(int, int)> selectionList = new List<(int, int)>();

    // used to keep track of the tile the user is mousing over
    private TileController mousedOverTile;

    private bool running = false;


    private IEnumerator waitTillControllerFree(UnitController uc)
    {
        yield return new WaitUntil(() => !uc.controllerBusy);

        if (sm.selectedUnit == uc && running)
        {
            enableMovementIndicators(steps);
            drawPath();
        }
    }

    public override void OnTileMouseDown(int tileX, int tileY)
    {
        if (steps == null || sm.selectedUnit.controllerBusy)
        {
            return;
        }


        if (mousedOverTile != null)
        {
            foreach (HexBoard.PathingStep step in steps)
            {
                if (step.endable && step.x == mousedOverTile.x && step.y == mousedOverTile.y)
                {
                    sm.boardManager.moveUnit(sm.selectedUnit, step);
                    sm.selectedUnit.unit.currentMovement = step.m;
                    break;
                }
            }
        }

        steps = sm.boardManager.getMoveableTiles(sm.selectedUnit);
        clearMovementIndicators();
        sm.StartCoroutine(waitTillControllerFree(sm.selectedUnit));
        drawPath();
    }

    public override void onTileMouseEnter(TileController tc)
    {
        mousedOverTile = tc;
        drawPath();
    }

    public override void onTileMouseExit(TileController tc)
    {
        if (mousedOverTile == tc)
        {
            mousedOverTile = null;
            drawPath();
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
        if (sm.selectedUnit != uc && uc.unit.team == playerTeam)
        {
            GameObject.FindObjectOfType<AudioManager>().Play("ClickPlayerUnit");
            sm.selectedUnit = uc;
            steps = sm.boardManager.getMoveableTiles(uc);
            drawPath();
            sm.hudController.DestroyAttackButtons();
            sm.hudController.CreateAttackButtons(uc.unit.attacks);

            clearMovementIndicators();
            sm.StartCoroutine(waitTillControllerFree(sm.selectedUnit));
        }
    }

    private void clearMovementIndicators()
    {
        foreach ((int x, int y) in selectionList)
        {
            sm.boardManager.tileControllers[x, y].setSelectionIndicatorState(false);
        }

        selectionList.Clear();
    }

    private void enableMovementIndicators(HexBoard.PathingStep[] steps)
    {
        clearMovementIndicators();

        // Enable the visualizer of the valid tiles
        foreach (HexBoard.PathingStep ps in steps)
        {
            if (ps.endable)
            {
                sm.boardManager.tileControllers[ps.x, ps.y].setSelectionIndicatorState(true);
                selectionList.Add((ps.x, ps.y));
            }
        }
    }

    public void drawPath()
    {
        /*if (sm.selectedUnit != null && sm.selectedUnit.controllerBusy)
        {
            return;
        } use this code if we want the line to stay while the unit walks*/

        sm.lineRenderer.positionCount = 0;
        if (mousedOverTile == null || steps == null || sm.selectedUnit.controllerBusy)
        {
            return;
        }

        HexBoard.PathingStep cur = null;
        foreach (HexBoard.PathingStep step in steps)
        {
            if (step.x == mousedOverTile.x && step.y == mousedOverTile.y)
            {
                cur = step;
                break;
            }
        }

        if (cur == null || !cur.endable)
        {
            return;
        }

        Vector3[] path = sm.boardManager.getMovementPath(cur);



        sm.lineRenderer.positionCount = path.Length;
        sm.lineRenderer.SetPositions(path);
    }

    // called when the selection controller will no longer be active, so it should clean up what it has done
    public override void onSelectionStopped()
    {
        running = false;
        sm.selectedUnit = null;
        steps = null;
        drawPath();
        clearMovementIndicators();
        sm.hudController.DestroyAttackButtons();
    }

    public override void onSelectionPaused()
    {
        running = false;
        clearMovementIndicators();
        sm.lineRenderer.positionCount = 0;
    }

    public override void onSelectionResumed()
    {
        running = true;
        if (sm.selectedUnit != null)
        {
            steps = sm.boardManager.getMoveableTiles(sm.selectedUnit);
            sm.StartCoroutine(waitTillControllerFree(sm.selectedUnit));
            foreach ((int x, int y) in selectionList)
            {
                sm.boardManager.tileControllers[x, y].setSelectionIndicatorState(true);
            }
            drawPath();
        }
    }

    public override void onSelectionStarted()
    {
        running = true;
    }

    // this means that the pathing data we have is old, so we will redo the pathing
    public override void onUnitListChanged()
    {
        clearMovementIndicators();
        if (sm.selectedUnit != null && !sm.selectedUnit.unit.isDead)
        {
            steps = sm.boardManager.getMoveableTiles(sm.selectedUnit);
            sm.StartCoroutine(waitTillControllerFree(sm.selectedUnit));
        }
        else
        {
            steps = null;
            sm.selectedUnit = null;
            sm.hudController.DestroyAttackButtons();
        }
        drawPath();
    }
}
