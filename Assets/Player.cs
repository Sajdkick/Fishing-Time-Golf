using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.green;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);

    }

    bool charging = false;
	// Update is called once per frame
	void Update () {

        float cameraDistance = Mathf.Abs(Camera.main.transform.position.z);

        if (!charging && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int layer_mask = LayerMask.GetMask("Player");
            //layer_mask = ~layer_mask;
            if (Physics.Raycast(ray, out hit, layer_mask))
            {
                if (hit.transform.gameObject.name == gameObject.name)
                {

                    charging = true;
                    Camera.main.GetComponent<CameraController>().enabled = false;

                }
                    
            }

        }
        if ( Input.GetMouseButton(0))
        {

            if (charging)
            {

                lineRenderer.SetPosition(0, transform.position);
                Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraDistance - 0.1f));
                lineRenderer.SetPosition(1, pos);

            }

        }

        if (charging && Input.GetMouseButtonUp(0))
        {

            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
            Camera.main.GetComponent<CameraController>().enabled = true;
            charging = false;

            Vector3 targetPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraDistance));
            Shoot(targetPos - transform.position, Vector3.Distance(transform.position, targetPos) * 3);

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

    }

}
