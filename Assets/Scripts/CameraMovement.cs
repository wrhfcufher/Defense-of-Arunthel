using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script that handles the camera controlls
/// </summary>
public class CameraMovement : MonoBehaviour
{

    private Transform cameraBase;
    [SerializeField]
    private float ROTSpeed = 10f;
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float panSpeed = 5f;


    private float ZoomAmount = 0; //With Positive and negative values
    [SerializeField]
    private float maxZoom = 10, minZoom = -2.5f;

    // Start is called before the first frame update
    void Start()
    {
        cameraBase = transform.parent.transform;
    }

    // Update is called once per frame
    void Update()
    {
        // find the forward direction of the camera
        Vector3 toMove = Vector3.zero;
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward = forward.normalized;

        // find the sideways direction of the camera
        Vector3 right = transform.right;
        right.y = 0;
        right = right.normalized;

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            toMove -= forward * moveSpeed; 
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            toMove += forward * moveSpeed;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            toMove -= right * moveSpeed;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            toMove += right * moveSpeed;
        }

        cameraBase.transform.position += toMove * Time.deltaTime;
        
        ZoomAmount += Input.GetAxis("Mouse ScrollWheel") * ROTSpeed;
        ZoomAmount = Mathf.Clamp(ZoomAmount, minZoom, maxZoom);

        // move camera to the correct position
        transform.position = cameraBase.transform.position + transform.forward * ZoomAmount;
        
        // camera rotation with right click
        if(Input.GetMouseButton(1))
        {
            cameraBase.transform.eulerAngles += panSpeed * new Vector3(0, Input.GetAxis("Mouse X"), 0);
        }

    }
  
}
