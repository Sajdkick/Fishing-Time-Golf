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
    int touchCount = 0;
    bool drag = false;

    void Start()
    {

        //Initialization our angles of camera
        xAngle = 0.0f;
        yAngle = 0.0f;
        this.transform.rotation = Quaternion.Euler(yAngle, xAngle, 0.0f);

    }

    // Update is called once per frame
    void Update () {

        //Here we make sure that we only can go up in touch count, only way to go down is to go directly to zero.
        //This makes sure that we dont start to drag the camera if we lift a finger after zooming.
        if (Input.touchCount > touchCount)
            touchCount = Input.touchCount;
        else if (Input.touchCount == 0)
            touchCount = 0;

        //We handle input differently on mobile and desktop.
        if (Application.isMobilePlatform)
        {
            if (touchCount == 1)
            {
                Drag();
            }
            else if (touchCount == 2 && Input.touchCount == 2)
            {
                if(zoomEnabled)
                    Zoom();
                Rotate();
            }
        }
        else
        {

            Drag();
            if (zoomEnabled)
                Zoom();

        }

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

            //We cast a ray that intersects with everything but the background.
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            LayerMask mask = 1 << 9;
            mask = ~mask;

            //If we didn't collide with anything, we know we want to start dragging.
            drag = !Physics.Raycast(ray, 100, mask);
            
        }
        
        else if (drag && Input.GetMouseButton(0))
        {

            //We calculate the new position of what we're dragging.
            float unitsPerPixel = Vector3.Distance(Camera.main.ViewportToWorldPoint(new Vector3(0, 0, transform.position.z)), Camera.main.ViewportToWorldPoint(new Vector3(1, 0, transform.position.z))) / Screen.width;
            Vector3 delta = (oldMousePosition - Input.mousePosition) * unitsPerPixel;
            Vector3 newPosition = transform.position + transform.right * delta.x + transform.up * delta.y;
            oldMousePosition = Input.mousePosition;

            float X = newPosition.x;
            float Y = newPosition.y;

            //We check if we want to lock any axes.
            if (lockX)
                X = transform.position.x;
            if (lockY)
                Y = transform.position.y;

            //If limitX == (0,0) it's unlimited.
            if (limitX.magnitude != 0)
            {

                //We clamp the X value between the limits.
                Mathf.Clamp(X, limitX.x, limitX.y);

            }

            if (limitY.magnitude != 0)
            {

                Mathf.Clamp(Y, limitY.x, limitY.y);

            }

            //We set the position.
            transform.position = new Vector3(X, Y, transform.position.z);

        }
        //If we let go, we stop dragging.
        else if (drag && Input.GetMouseButtonUp(0))
            drag = false;

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

    private Vector3 firstpoint;
    private Vector3 secondpoint;
    private float xAngle = 0.0f; 
    private float yAngle = 0.0f;
    private float xAngTemp = 0.0f;
    private float yAngTemp = 0.0f;
    void Rotate()
    {
        //Check count touches
        if (Input.touchCount > 0)
        {
            //Touch began, save position
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                firstpoint = Input.GetTouch(0).position;
                xAngTemp = xAngle;
                yAngTemp = yAngle;
            }
            //Move finger by screen
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                secondpoint = Input.GetTouch(0).position;
                //Mainly, about rotate camera. For example, for Screen.width rotate on 180 degree
                xAngle = xAngTemp + (secondpoint.x - firstpoint.x) * 180.0f / Screen.width;
                yAngle = yAngTemp - (secondpoint.y - firstpoint.y) * 90.0f / Screen.height;
                //Rotate camera
                if(pivotObject == null)
                    transform.rotation = Quaternion.Euler(0, 0, xAngle);
                else pivotObject.transform.rotation = Quaternion.Euler(0, 0, xAngle);
            }
        }
    }
}
