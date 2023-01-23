using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// All the mouse input events get sent here. This class then redirects those inputs to different selection controllers.
/// Each selection controller handles different input modes (selecting units, different attacks, etc).
/// New controller get added to a stack allowing them for the program to easily switch between different input modes.
/// </summary>

[RequireComponent(typeof(LineRenderer))]
public class SelectionManager : MonoBehaviour
{
    [SerializeField]
    public HUDController hudController;
    [SerializeField]
    public BoardManager boardManager;
    [SerializeField]
    public TurnManager turnManager;

    [HideInInspector]
    public LineRenderer lineRenderer;

    // used to keep track of the selectedUnit across multiple SelectionControllers
    public UnitController selectedUnit;

    // stack of selection controllers and the callback to use when the controller is finished
    private List<(SelectionController, Action)> interruptingSelectionControllers = new List<(SelectionController, Action)>();

    [SerializeReference]
    // I'm not sure how to make a custom editor for polymorphic types like this, so im just going to add one of each by default and we can remove the ones we don't need in the editor.
    // In the future I think this will be given by the initilizer or turn manager anyways
    public SelectionController[] selectionControllers = new SelectionController[] { new PlayerTurnSelectionController(), new EnemyTurnSelectionController() };

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        foreach (SelectionController selectionController in selectionControllers)
        {
            selectionController.sm = this;
        }

        selectionControllers[turnManager.turnCount % selectionControllers.Length].onSelectionStarted();
    }

    // Methods called by tile and unit controllers when mouse events happen
    public void OnUnitMouseDown(UnitController uc)
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; } // if hovering over a UI element done use this event
        if (interruptingSelectionControllers.Count == 0)
        {
            selectionControllers[turnManager.turnCount % selectionControllers.Length].OnUnitMouseDown(uc);
        }
        else
        {
            interruptingSelectionControllers[0].Item1.OnUnitMouseDown(uc);
        }
    }

    public void OnUnitMouseUp(UnitController uc)
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; } // if hovering over a UI element done use this event
        if (interruptingSelectionControllers.Count == 0)
        {
            selectionControllers[turnManager.turnCount % selectionControllers.Length].OnUnitMouseUp(uc);
        }
        else
        {
            interruptingSelectionControllers[0].Item1.OnUnitMouseUp(uc);
        }
    }

    public void OnTileMouseDown(int tileX, int tileY)
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; } // if hovering over a UI element done use this event

        if (interruptingSelectionControllers.Count == 0)
        {
            selectionControllers[turnManager.turnCount % selectionControllers.Length].OnTileMouseDown(tileX, tileY);
        }
        else
        {
            interruptingSelectionControllers[0].Item1.OnTileMouseDown(tileX, tileY);
        }
    }

    public void OnTileMouseUp(int tileX, int tileY)
    {
        if (interruptingSelectionControllers.Count == 0)
        {
            selectionControllers[turnManager.turnCount % selectionControllers.Length].OnTileMouseUp(tileX, tileY);
        }
        else
        {
            interruptingSelectionControllers[0].Item1.OnTileMouseUp(tileX, tileY);
        }
    }

    public void onTileMouseEnter(TileController tc)
    {
        hudController.notifyTileHovered(tc);
        if (interruptingSelectionControllers.Count == 0)
        {
            selectionControllers[turnManager.turnCount % selectionControllers.Length].onTileMouseEnter(tc);
        }
        else
        {
            interruptingSelectionControllers[0].Item1.onTileMouseEnter(tc);
        }
    }

    public void onTileMouseExit(TileController tc)
    {
        hudController.notifyTileUnhovered();
        if (interruptingSelectionControllers.Count == 0)
        {
            selectionControllers[turnManager.turnCount % selectionControllers.Length].onTileMouseEnter(tc);
        }
        else
        {
            interruptingSelectionControllers[0].Item1.onTileMouseExit(tc);
        }
    }

    public void addInterruptingSelectionController(SelectionController sc, Action removalCallback = null, bool removeOtherInterrupters = true)
    {

        sc.sm = this;
        if (interruptingSelectionControllers.Count != 0)
        {
            interruptingSelectionControllers[0].Item1.onSelectionPaused();
        }
        else
        {
            selectionControllers[turnManager.turnCount % selectionControllers.Length].onSelectionPaused();
        }

        if (removeOtherInterrupters)
        {
            while (interruptingSelectionControllers.Count > 0)
            {
                interruptingSelectionControllers[0].Item1.onSelectionStopped();
                if (interruptingSelectionControllers[0].Item2 != null)
                {
                    interruptingSelectionControllers[0].Item2();
                }
                interruptingSelectionControllers.RemoveAt(0);
            }

        }

        interruptingSelectionControllers.Insert(0, (sc, removalCallback));
        sc.onSelectionStarted();
    }

    public void removeInterruptingSelectionController(SelectionController sc)
    {

        for (int i = 0; i < interruptingSelectionControllers.Count; i++)
        {
            if (interruptingSelectionControllers[i].Item1 == sc)
            {
                sc.onSelectionStopped();
                if (interruptingSelectionControllers[i].Item2 != null)
                {
                    interruptingSelectionControllers[i].Item2();
                }
                interruptingSelectionControllers.RemoveAt(i);
                if (i == 0)
                {
                    if (interruptingSelectionControllers.Count != 0)
                    {
                        interruptingSelectionControllers[0].Item1.onSelectionResumed();
                    }
                    else
                    {
                        selectionControllers[turnManager.turnCount % selectionControllers.Length].onSelectionResumed();
                    }
                }
            }
        }
    }

    private void onUnitListChange()
    {
        if (interruptingSelectionControllers.Count == 0)
        {
            selectionControllers[turnManager.turnCount % selectionControllers.Length].onUnitListChanged();
        }
        else
        {
            interruptingSelectionControllers[0].Item1.onUnitListChanged();
        }
    }

    public void subscribeEndOfTurn(TurnManager tm)
    {
        tm.addEndOfTurnListener(onTurnEnd);
    }

    public void subscribeOnUnitListChange()
    {
        boardManager.addOnUnitListChangeListener(onUnitListChange);
    }

    // Method that will be called when the turn ends through an event
    private void onTurnEnd()
    {
        foreach ((SelectionController sc, Action callback) in interruptingSelectionControllers)
        {
            sc.onSelectionStopped();
            if (callback != null)
            {
                callback();
            }
        }

        interruptingSelectionControllers.Clear();

        selectionControllers[turnManager.turnCount % selectionControllers.Length].onSelectionStopped();
        selectionControllers[(turnManager.turnCount + 1) % selectionControllers.Length].onSelectionResumed();
    }

}
