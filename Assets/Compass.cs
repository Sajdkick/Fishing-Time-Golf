using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour {

	// Use this for initialization
	void Start () {

        transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0.8f, 0.8f, 2));

	}
	
	// Update is called once per frame
	void Update () {

        transform.LookAt(transform.position + Vector3.up);

	}
}
