using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;
using UnityEngine.UI;

public class CourtCreator : MonoBehaviour {

    List<Vector2d> points;
    ILocationProvider locationProvider;

    GameObject lineObject;
    LineRenderer lineRenderer;
    List<Vector3> linePoints;

    Vector2d lastPoint;
    // Use this for initialization
    void Start () {

        locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;

        points = new List<Vector2d>();
        points.Add(locationProvider.Location);
        lastPoint = points[0];
        linePoints.Add(transform.position);

        lineObject = new GameObject("Court Line");
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.red;
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);

    }
	
	// Update is called once per frame
	void Update () {

        Vector2d location = locationProvider.Location;
        if ((Conversions.LatLonToMeters(lastPoint) - Conversions.LatLonToMeters(location)).magnitude > 50){
            points.Add(location);
            lastPoint = location;
            linePoints.Add(transform.position);

            lineRenderer.positionCount = linePoints.Count;
            lineRenderer.SetPositions(linePoints.ToArray());

        }

	}

    void FinishCourt()
    {



    }

}
