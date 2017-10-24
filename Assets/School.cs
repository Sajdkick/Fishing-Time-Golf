using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class School : MonoBehaviour {

    public float fleeChance = 0.75f;
    public WaterObjectTile tile;

	// Use this for initialization
	void Start () {
        GetComponent<SphereCollider>().isTrigger = true;
	}

    float minDistance = -1;
	// Update is called once per frame
	void Update () {
		
        if(Player.GetActiveBall() != null)
        {

            if(minDistance < 0)
                minDistance = float.MaxValue;

            float distance = Vector3.Distance(Player.GetActiveBall().transform.position, transform.position);
            if (distance < minDistance)
                minDistance = distance;

        }
        else if(minDistance >= 0 && minDistance < 0.3f)
        {

            float diceRoll = Random.value;
            float threshold = Mathf.Lerp(0, fleeChance, 1 - (minDistance / 0.3f));

            if (diceRoll < threshold)
                Remove();

            minDistance = -1;

        }

	}

    void OnTriggerEnter(Collider collider)
    {

        if(collider.gameObject.tag == "ball")
        {

            Player.GivePoint(1);
            Remove();

        }

    }

    void Remove()
    {

        tile.RemoveObject(gameObject);
        Destroy(gameObject);

    }

}
