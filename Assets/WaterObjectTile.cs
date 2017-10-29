using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;
using UnityEngine.UI;

public class WaterObjectTile : MonoBehaviour{

    UnwrappedTileId tileID;

    Material obstacleMaterial;
    Material schoolMaterial;

    public class WaterObject {

        public enum Type
        {

            OBSTACLE = 1,
            SCHOOL

        }

        public int x;
        public int y;
        public GameObject gameObject;

    }

    public List<WaterObject> waterObjects;

    public void Awake()
    {

        obstacleMaterial = new Material(Shader.Find("Unlit/Color"));
        obstacleMaterial.color = Color.black;
        schoolMaterial = new Material(Shader.Find("Unlit/Color"));
        schoolMaterial.color = Color.yellow;

    }

    public void Initialize(UnwrappedTileId _tileID)
    {

        tileID = _tileID;
        //We generate all the obstacles.

        waterObjects = new List<WaterObject>();

        Vector3 tileCorner = transform.position - Vector3.right * 0.5f - Vector3.up * 0.5f;

        Random.InitState(tileID.GetHashCode());
        for (int i = 0; i < 20; i++)
        {

            WaterObject newObject = new WaterObject();
            newObject.x = Random.Range(0, 255);
            newObject.y = Random.Range(0, 255);

            if (Random.Range(1, 10) == 9)
            {
                newObject.gameObject = SpawnSchool(tileCorner + new Vector3(newObject.x / 255.0f, 1 - (newObject.y / 255.0f)));
            }
            else
            {

                newObject.gameObject = SpawnObstacle(tileCorner + new Vector3(newObject.x / 255.0f, 1 - (newObject.y / 255.0f)));

            }

            newObject.gameObject.SetActive(false);
            waterObjects.Add(newObject);

        }

    }

    public void RemoveObject(GameObject target)
    {

        foreach(WaterObject waterObject in waterObjects)
        {

            if(waterObject.gameObject == target)
            {

                waterObjects.Remove(waterObject);
                return;

            }

        }

    }

    GameObject SpawnSchool(Vector3 position)
    {

        GameObject waterObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        waterObject.transform.position = position;
        waterObject.transform.localScale *= 0.05f;
        waterObject.transform.parent = transform.GetChild(0);

        Rigidbody rigidbody = waterObject.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        waterObject.GetComponent<MeshRenderer>().material = schoolMaterial;
        waterObject.AddComponent<School>().tile = this;

        return waterObject;

    }
    GameObject SpawnObstacle(Vector3 position)
    {

        GameObject waterObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        waterObject.transform.position = position;
        waterObject.transform.localScale *= 0.05f;
        waterObject.transform.parent = transform.GetChild(0);

        Rigidbody rigidbody = waterObject.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        waterObject.GetComponent<SphereCollider>().material = Resources.Load("Obstacle") as PhysicMaterial;

        waterObject.GetComponent<MeshRenderer>().material = obstacleMaterial;

        return waterObject;

    }


}
