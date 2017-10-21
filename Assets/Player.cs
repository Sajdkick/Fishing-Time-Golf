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
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);

    }

    bool charging = false;
	// Update is called once per frame
	void Update () {

        if (!charging && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit))
            {
                
                if (hit.transform.gameObject.name == gameObject.name)
                    charging = true;
            }

        }
        if (charging && Input.GetMouseButton(0))
        {

            lineRenderer.SetPosition(0, transform.position);
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.9f));
            lineRenderer.SetPosition(1, pos);

        }

        if (charging && Input.GetMouseButtonUp(0))
        {

            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
            charging = false;

            Shoot(transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2f)), Vector3.Distance(transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2f))) * 100);

        }

    }

    void Shoot(Vector3 direction, float force)
    {

        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Rigidbody rigidbody = ball.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        rigidbody.AddForce(direction.normalized * force,ForceMode.Impulse);
        rigidbody.useGravity = false;

        ball.AddComponent<Ball>();

    }

}
