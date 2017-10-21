using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes the player sprite follow the point which we move.
public class LerpPosition : MonoBehaviour {

    /// <summary>
    /// Which gameobject to lerp agains.
    /// </summary>
    public GameObject target;

    /// <summary>
    /// Lerp factor.
    /// </summary>
    public float t;

    /// <summary>
    /// Make sure GameObject allways is inside the max distance.
    /// </summary>
    public float maxDistance;

    /// <summary>
    /// Z value remains constant.
    /// </summary>
    public bool lockZ;

	// Update is called once per frame
	void LateUpdate () {

        float oldZ = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, target.transform.position, t);

        if(Vector3.Distance(transform.position, target.transform.position) > maxDistance)
        {

            transform.position = (transform.position - target.transform.position).normalized * maxDistance + target.transform.position;

        }

        if (lockZ)
            transform.position = new Vector3(transform.position.x, transform.position.y, oldZ);

	}
}
