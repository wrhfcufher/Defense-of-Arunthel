using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Script on the units inside th level maker scene
/// </summary>
public class LevelMakerUnit : MonoBehaviour
{

    [SerializeField]
    private GameObject selectionIndicator;

    public int team, index;
    public LevelMaker levelMaker;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; } // if hovering over a UI element does not cause the tile to be clicked

        setSelectionIndicatorState(true);
        levelMaker.onUnitSelected(this);
    }

    public void setSelectionIndicatorState(bool state)
    {
        selectionIndicator.SetActive(state);
    }
}
