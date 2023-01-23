using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Simple script to copy the cameras rotation in order to keep 
///  the healthbars facing in the correct direction
/// </summary>
public class Billboard : MonoBehaviour
{

    private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        target = Camera.main.transform;
    }



    void Update()
    {
        // Rotate the camera every frame so it keeps looking at the target
        // transform.LookAt(target);
        transform.rotation = target.transform.rotation;
    }
}
