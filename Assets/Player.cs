using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    static GameObject activeBall;
    static public GameObject GetActiveBall() { return activeBall; }

    static int score = 0;
    static public void GivePoint(int point) { score += point; }
    static public int GetScore() { return score; }

    LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.green;
        lineRenderer.widthMultiplier = 0.075f;
        lineRenderer.startWidth = 0;
        lineRenderer.endWidth = 0.075f;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);

    }

    bool charging = false;
    float chargeMeter = 0;
    float doubleClickTimer = -1;
    Vector3 startChargePosition;
	// Update is called once per frame
	void Update () {

        float cameraDistance = Mathf.Abs(Camera.main.transform.position.z);

        if (!charging && DoubleClick() && activeBall == null)
        {
            RaycastHit hit;

            int layer_mask = LayerMask.GetMask("Player");

            if(Physics.Raycast(transform.position + -Vector3.forward, Vector3.forward, out hit, 10, ~layer_mask))
            {

                if(hit.collider.gameObject.layer == 9)
                {

                    Renderer rend = hit.transform.GetComponent<Renderer>();
                    Texture2D tex = rend.material.mainTexture as Texture2D;
                    Vector3 pixelUV = hit.textureCoord;
                    pixelUV.x *= tex.width;
                    pixelUV.y *= tex.height;

                    Color color = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
                    if(color != Color.blue)
                    {

                        charging = true;
                        startChargePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraDistance));
                        Camera.main.GetComponent<CameraController>().enabled = false;

                    }

                }

            }


        }
        if (Input.GetMouseButton(0))
        {

            if (charging)
            {

                lineRenderer.SetPosition(0, startChargePosition);
                Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraDistance - 0.1f));

                chargeMeter = Mathf.Clamp(Vector3.Distance(startChargePosition, pos), 0, 0.5f);

                Color chargeColor = Color.Lerp(Color.green, Color.red, chargeMeter / 0.5f);

                lineRenderer.material.color = chargeColor;
                lineRenderer.SetPosition(1, startChargePosition + (pos - startChargePosition).normalized * chargeMeter);

            }

        }

        if (charging && Input.GetMouseButtonUp(0))
        {

            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
            Camera.main.GetComponent<CameraController>().enabled = true;
            charging = false;

            Vector3 targetPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraDistance));
            if(chargeMeter > 0.05)
                Shoot(startChargePosition - targetPos, chargeMeter * 6);

        }

    }

    void Shoot(Vector3 direction, float force)
    {

        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.position = transform.position;
        ball.transform.localScale = transform.localScale * 0.5f;
        Physics.IgnoreCollision(ball.GetComponent<SphereCollider>(), GetComponent<SphereCollider>());

        Rigidbody rigidbody = ball.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        rigidbody.AddForce(direction.normalized * force,ForceMode.Impulse);
        rigidbody.useGravity = false;

        ball.AddComponent<Ball>();
   
        activeBall = ball;

    }
    
    bool DoubleClick()
    {
        Debug.Log(doubleClickTimer);
        if (Input.GetMouseButtonDown(0))
        {

            if (doubleClickTimer == -1)
                doubleClickTimer = Time.deltaTime;
            else if (doubleClickTimer < 0.5f)
            {

                doubleClickTimer = -1;
                return true;

            }

        }

        if (doubleClickTimer != -1)
            doubleClickTimer += Time.deltaTime;
        if (doubleClickTimer > 0.5f)
            doubleClickTimer = -1;

        return false;

    }

}
