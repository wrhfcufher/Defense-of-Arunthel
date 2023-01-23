using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Script used to create the hexgon manualy like we did in class at the start of the semester
/// </summary>

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshMaker : MonoBehaviour
{

    public bool saveMesh = false;
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh createdMesh = MakeMesh();
        mf.mesh = createdMesh;

        if (saveMesh)
        {
            #if (UNITY_EDITOR) // this only works in the editor
            AssetDatabase.CreateAsset(createdMesh, "Assets/createdMesh.asset");
            #endif
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Mesh MakeMesh()
    {
        Mesh toReturn = new Mesh();
        Vector3[] verts = {
        //top
        new Vector3(-0.5f, 1, 0),
        new Vector3(-0.25f, 1, 0.5f),
        new Vector3(-0.25f, 1, -0.5f),

        //new Vector3(-0.25f, 1, 0.5f), reuse
        new Vector3(0.25f, 1, -0.5f),
        // new Vector3(-0.25f, 1, -0.5f), reuse

        //new Vector3(-0.25f, 1, 0.5f), reuse
        new Vector3(0.25f, 1, 0.5f),
        //new Vector3(0.25f, 1, -0.5f), reuse

        // new Vector3(0.25f, 1, 0.5f), reuse
        new Vector3(0.5f, 1, 0),
        //new Vector3(0.25f, 1, -0.5f) reuse


        // bottom
        new Vector3(-0.5f, 0, 0),
        new Vector3(-0.25f, 0, 0.5f),
        new Vector3(-0.25f, 0, -0.5f),

        //new Vector3(-0.25f, 0, 0.5f), reuse
        new Vector3(0.25f, 0, -0.5f),
        // new Vector3(-0.25f, 0, -0.5f), reuse

        //new Vector3(-0.25f, 0, 0.5f), reuse
        new Vector3(0.25f, 0, 0.5f),
        //new Vector3(0.25f, 0, -0.5f), reuse

        // new Vector3(0.25f, 0, 0.5f), reuse
        new Vector3(0.5f, 0, 0),
        //new Vector3(0.25f, 0, -0.5f) reuse

        // sides (need seperate vertecies for seprate normals)
        new Vector3(-0.5f, 1, 0),
        new Vector3(-0.5f, 0, 0),
        new Vector3(-0.25f, 1, -0.5f),
        new Vector3(-0.25f, 0, -0.5f),

        new Vector3(-0.25f, 1, -0.5f),
        new Vector3(-0.25f, 0, -0.5f),
        new Vector3(0.25f, 1, -0.5f),
        new Vector3(0.25f, 0, -0.5f),

        new Vector3(0.25f, 1, -0.5f),
        new Vector3(0.25f, 0, -0.5f),
        new Vector3(0.5f, 1, 0),
        new Vector3(0.5f, 0, 0),

        new Vector3(0.5f, 1, 0),
        new Vector3(0.5f, 0, 0),
        new Vector3(0.25f, 1, 0.5f),
        new Vector3(0.25f, 0, 0.5f),


        new Vector3(0.25f, 1, 0.5f),
        new Vector3(0.25f, 0, 0.5f),
        new Vector3(-0.25f, 1, 0.5f),
        new Vector3(-0.25f, 0, 0.5f),


        new Vector3(-0.25f, 1, 0.5f),
        new Vector3(-0.25f, 0, 0.5f),
        new Vector3(-0.5f, 1, 0),
        new Vector3(-0.5f, 0, 0),

        };

        Vector2[] uvs = {
        //top
        new Vector2(0, 0.5f),
        new Vector2(0.125f, 1),
        new Vector2(0.125f, 0),

        //new Vector3(-0.25f, 1, 0.5f), reuse
        new Vector2(0.375f, 0),
        // new Vector3(-0.25f, 1, -0.5f), reuse

        //new Vector3(-0.25f, 1, 0.5f), reuse
        new Vector2(0.375f, 1),
        //new Vector3(0.25f, 1, -0.5f), reuse

        // new Vector3(0.25f, 1, 0.5f), reuse
        new Vector2(0.5f, 0.5f),
        //new Vector3(0.25f, 1, -0.5f) reuse


        // bottom
        new Vector2(0, 0.5f),
        new Vector2(0.125f, 1),
        new Vector2(0.125f, 0),

        //new Vector3(-0.25f, 0, 0.5f), reuse
        new Vector2(0.375f, 0),
        // new Vector3(-0.25f, 0, -0.5f), reuse

        //new Vector3(-0.25f, 0, 0.5f), reuse
        new Vector2(0.375f, 1),
        //new Vector3(0.25f, 0, -0.5f), reuse

        // new Vector3(0.25f, 0, 0.5f), reuse
        new Vector2(0.5f, 0.5f),
        //new Vector3(0.25f, 0, -0.5f) reuse

        // sides (need seperate vertecies for seprate normals)
        new Vector3(0.5f, 1),
        new Vector3(0.5f, 0),
        new Vector3(1, 1),
        new Vector3(1, 0),

        new Vector3(0.5f, 1),
        new Vector3(0.5f, 0),
        new Vector3(1, 1),
        new Vector3(1, 0),

        new Vector3(0.5f, 1),
        new Vector3(0.5f, 0),
        new Vector3(1, 1),
        new Vector3(1, 0),

        new Vector3(0.5f, 1),
        new Vector3(0.5f, 0),
        new Vector3(1, 1),
        new Vector3(1, 0),


        new Vector3(0.5f, 1),
        new Vector3(0.5f, 0),
        new Vector3(1, 1),
        new Vector3(1, 0),


        new Vector3(0.5f, 1),
        new Vector3(0.5f, 0),
        new Vector3(1, 1),
        new Vector3(1, 0),

        };

        // make a list of garbage normals (they will be calculated later)
        Vector3[] norms = new Vector3[verts.Length];
        for (int i = 0; i < norms.Length; i++)
        {
            norms[i] = Vector3.up;
        }
        int[] tris = new int[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            tris[i] = i;
        }

        toReturn.vertices = verts;
        toReturn.uv = uvs;
        toReturn.normals = norms;
        toReturn.triangles = new int[] {// top
                                        0, 1, 2,
                                        1, 3, 2,
                                        1, 4, 3,
                                        4, 5, 3,
                                        // bottom
                                        8, 7, 6,
                                        8, 9, 7,
                                        9, 10, 7,
                                        9, 11, 10,
                                        // side 1
                                        14, 13, 12,
                                        13, 14, 15,
                                        // side 2
                                        18, 17, 16,
                                        17, 18, 19,
                                        //side 3
                                        22, 21, 20,
                                        21, 22, 23,
                                        //side 4
                                        26, 25, 24,
                                        25, 26, 27,
                                        //side 5
                                        30, 29, 28,
                                        29, 30, 31,
                                        //side 6
                                        34, 33, 32,
                                        33, 34, 35};
        toReturn.RecalculateNormals(); // this is a simple shape, so let unity calculate the normals for us
        toReturn.RecalculateBounds();
        toReturn.name = "Hexagon";

        return toReturn;
    }
}
