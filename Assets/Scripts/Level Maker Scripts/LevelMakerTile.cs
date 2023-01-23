using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// Script on the tiles inside the level maker scene
/// </summary>

[RequireComponent(typeof(MeshFilter))]
public class LevelMakerTile : MonoBehaviour
{
    [SerializeField]
    private GameObject selectionIndicator;

    public int x, y;
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
        levelMaker.onTileSelected(x, y);
    }

    public void resizeMesh(int height)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Vector3[] verts = mf.mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            if (verts[i].y != 0)
            {
                verts[i].y = 1 + height / 10f;
            }
        }
        mf.sharedMesh.vertices = verts;
        mf.sharedMesh.RecalculateBounds();
        GetComponent<MeshCollider>().sharedMesh = mf.mesh;

        // move the selection indicator to the new height
        selectionIndicator.transform.position = new Vector3(selectionIndicator.transform.position.x, 1 + height / 10f, selectionIndicator.transform.position.z);
    }

    public void setSelectionIndicatorState(bool state)
    {
        selectionIndicator.SetActive(state);
    }
}
