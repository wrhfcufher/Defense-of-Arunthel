using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The SelectionController abstract class. Defines methods and fields that the child classes need to implement
/// </summary>

[System.Serializable]
public abstract class SelectionController
{
    public SelectionManager sm;

    // Methods called by tile and unit controllers when mouse events happen
    public abstract void OnUnitMouseDown(UnitController uc);

    public abstract void OnUnitMouseUp(UnitController uc);

    public abstract void OnTileMouseDown(int tileX, int tileY);

    public abstract void OnTileMouseUp(int tileX, int tileY);

    public abstract void onTileMouseEnter(TileController tc);

    public abstract void onTileMouseExit(TileController tc);

    public abstract void onSelectionStopped();

    public abstract void onSelectionPaused();
    public abstract void onSelectionResumed();
    public abstract void onSelectionStarted();

    public abstract void onUnitListChanged();
}
