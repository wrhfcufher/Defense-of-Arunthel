using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scripts put on tile gameobjects. It holds a refrence to the board manager and a tile x and y pos (allowing it to know what tile it is representing).
/// It sends input events to the selectionManager in order to allow the game to respond to inputs.
/// </summary>
public class TileController : MonoBehaviour
{

    [SerializeField]
    private GameObject selectionIndicator;
    
    private BoardManager _boardManager;
    private SelectionManager _selectionManager;
    private int _x = -1, _y = -1;
    public BoardManager boardManager
    {
        get { return _boardManager; }
        set
        {
            if (_boardManager == null)
            {
                _boardManager = value;
            }
            else
            {
                throw new System.Exception("Tile Controller was assigned a boardManager while already having one");
            }
        }
    }

    public SelectionManager selectionManager
    {
        get { return _selectionManager; }
        set
        {
            if (_selectionManager == null)
            {
                _selectionManager = value;
            }
            else
            {
                throw new System.Exception("Tile Controller was assigned a selectionManager while already having one");
            }
        }
    }

    public Tile tile { get { return _boardManager.getTile(x, y); } }

    public int x
    {
        get { return _x; }
        set
        {
            if (_x == -1)
            {
                _x = value;
            }
            else
            {
                throw new System.Exception("Tile controllers x value can not be assigned twice");
            }
        }
    }

    public int y
    {
        get { return _y; }
        set
        {
            if (_y == -1)
            {
                _y = value;
            }
            else
            {
                throw new System.Exception("Tile controllers y value can not be assigned twice");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (boardManager == null)
        {
            Debug.LogWarning("No boardManager has been assigned to tile controller. This will cause errors.");
        }

        if (selectionManager == null)
        {
            Debug.LogWarning("No selectionManager has been assigned to tile controller. This will cause errors.");
        }

        if (x == -1 || y == -1)
        {
            Debug.LogWarning("The tile controllers x and y values have not been properly initalized.");
        }

        if (selectionIndicator == null)
        {
            Debug.LogWarning("The tile controllers selectionIndicator has not been properly initalized.");
        }

        resizeMesh();
    }

    // resize the mesh to the correct size (based off of the tile height)
    private void resizeMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Vector3[] verts = mf.mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            if (verts[i].y != 0)
            {
                verts[i].y = 1 + tile.height / 10f;
            }
        }
        mf.sharedMesh.vertices = verts;
        mf.sharedMesh.RecalculateBounds();
        GetComponent<MeshCollider>().sharedMesh = mf.mesh;

        // move the selection indicator to the new height
        selectionIndicator.transform.position = new Vector3(selectionIndicator.transform.position.x, 1 + tile.height / 10f, selectionIndicator.transform.position.z);
    }

    private void OnMouseDown()
    {
        selectionManager.OnTileMouseDown(x, y);
    }

    private void OnMouseUp()
    {
        selectionManager.OnTileMouseUp(x, y);
    }

    private void OnMouseEnter()
    {
        selectionManager.onTileMouseEnter(this);
    }

    private void OnMouseExit()
    {
        selectionManager.onTileMouseExit(this);
    }

    public void setSelectionIndicatorState(bool state)
    {
        selectionIndicator.SetActive(state);
    }

}
