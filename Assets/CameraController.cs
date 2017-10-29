using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add to a camera to enable dragging it.
/// </summary>
public class CameraController : MonoBehaviour {

    /// <summary>
    /// Lock movement along the X axis.
    /// </summary>
    public bool lockX = false;

    /// <summary>
    /// Lock movement along the Y axis.
    /// </summary>
    public bool lockY = false;

    /// <summary>
    /// How far the camera can move along the X axis.
    /// </summary>
    public Vector2 limitX;

    /// <summary>
    /// How far the camera can move along the Y axis.
    /// </summary>
    public Vector2 limitY;

    /// <summary>
    /// Can we zoom?
    /// </summary>
    public bool zoomEnabled = true;

    /// <summary>
    /// The object which we rotate around, if null we use the camera.
    /// </summary>
    public GameObject pivotObject;

    //Internal variables used for calculations.
    Vector3 oldMousePosition;
    Vector3 oldCameraPosition;
    int touchCount = 0;
    bool drag = false;

    void Start()
    {

    }

    // Update is called once per frame
    void Update () {

        //Here we make sure that we only can go up in touch count, only way to go down is to go directly to zero.
        //This makes sure that we dont start to drag the camera if we lift a finger after zooming.
        Drag();

    }

    /// <summary>
    /// Handles the moving of the camera.
    /// </summary>
    void Drag()
    {
        //If we pressed down this frame (GetMouseButtonDown also works for touch input)
        if (!drag && Input.GetMouseButtonDown(0))
        {

            //We set the initial value of oldMousePosition.
            oldMousePosition = Input.mousePosition;

            drag = true;
            
        }
        else if (drag && Input.GetMouseButton(0))
        {

            //We calculate the new position of what we're dragging.
            float unitsPerPixel = Vector3.Distance(Camera.main.ViewportToWorldPoint(new Vector3(0, 0, transform.position.z)), Camera.main.ViewportToWorldPoint(new Vector3(1, 0, transform.position.z))) / Screen.width;
            Vector3 delta = (oldMousePosition - Input.mousePosition) * unitsPerPixel;
            Vector3 newPosition = transform.position + transform.up * delta.y;

            float X = newPosition.x;
            float Y = newPosition.y;

            //We set the position.
            transform.position = new Vector3(X, Y, transform.position.z);

            if(pivotObject != null)
            {

                Vector3 pivotInViewport = GetComponent<Camera>().WorldToViewportPoint(pivotObject.transform.position);
                if (pivotInViewport.x > 1 || pivotInViewport.x < 0 || pivotInViewport.y > 1 || pivotInViewport.y < 0)
                    transform.position = oldCameraPosition;
                else
                {

                    Rotate();

                }

            }

        }
        //If we let go, we stop dragging.
        else if (drag && Input.GetMouseButtonUp(0))
            drag = false;

        oldMousePosition = Input.mousePosition;
        oldCameraPosition = transform.position;

    }

    /// <summary>
    /// Handles the zooming of the camera.
    /// </summary>
    void Zoom()
    {
            
        //We zoom by changing the size of the camera.

        if (Application.isMobilePlatform)
        {

            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            Camera camera = Camera.main;

            // ... change the orthographic size based on the change in distance between the touches.
            camera.orthographicSize += deltaMagnitudeDiff * 0.02f;

            // Make sure the orthographic size never drops below zero.
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, 1f, 100f);

        }
        else
        {

            Camera camera = Camera.main;

            // ... change the orthographic size based on the change in distance between the touches.
            camera.orthographicSize += Input.GetAxis("Mouse ScrollWheel") * 3;

            // Make sure the orthographic size never drops below zero.
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, 1f, 100f);

        }

    }

    private float zAngle = 0.0f; 
    private float zAngTemp = 0.0f;
    void Rotate()
    {

        //Move finger by screen
        if (Input.GetMouseButton(0))
        {

            if (oldMousePosition != Input.mousePosition)
            {

                Vector3 mousePoint = Camera.main.ScreenToWorldPoint(new Vector3(oldMousePosition.x, oldMousePosition.y, pivotObject.transform.position.z));
                Vector3 pivotToMouse = mousePoint - pivotObject.transform.position;
                float direction = Mathf.Sign(Vector3.Dot(pivotToMouse, Camera.main.transform.up));
                //Mainly, about rotate camera. For example, for Screen.width rotate on 180 degree

                if (direction == -1)
                    zAngle = (oldMousePosition.x - Input.mousePosition.x) * 180.0f / Screen.width;
                else zAngle = (Input.mousePosition.x - oldMousePosition.x) * 180.0f / Screen.width;

                //Rotate camera
                pivotObject.transform.Rotate(0,0,zAngle);

            }
        }
    }
}
