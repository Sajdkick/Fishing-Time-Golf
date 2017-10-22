using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

	// Use this for initialization
	void Start () {

        gameObject.tag = "ball";

	}

    float lifetime = 4;
	// Update is called once per frame
	void Update () {

        lifetime -= Time.deltaTime;

        if (lifetime < 0)
            Destroy(gameObject);

	}
}
