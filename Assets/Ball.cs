using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    float lifetime = 10;
	// Update is called once per frame
	void Update () {

        lifetime -= Time.deltaTime;

        if (lifetime < 0)
            Destroy(gameObject);

	}
}
