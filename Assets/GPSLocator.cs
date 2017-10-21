using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;

public class GPSLocator : MonoBehaviour {

    public int zoom;
    public string mapID;
    public int gridSize;

    public GameObject player;

    MapGrid grid;

    ILocationProvider locationProvider;

	// Use this for initialization
	void Start () {
        locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        Vector2d location = locationProvider.Location;
        grid = new MapGrid((float)location.x, (float)location.y, zoom, mapID, gridSize);
    }
	
	// Update is called once per frame
	void Update () {
        Vector2d location = locationProvider.Location;
        if (Input.GetKeyDown(KeyCode.Space))
            grid.GenerateGrid(56.182f, 15.59f, zoom);

        if (grid.AllTilesLoaded())
        {

            UnwrappedTileId closestTileId = grid.GetClosestTile((float)location.x, (float)location.y).tileID;
            UnwrappedTileId centerTileId = grid.GetCenterTile().tileID;

            if (closestTileId.X != centerTileId.X || closestTileId.Y != centerTileId.Y)
            {

                int x = closestTileId.X - centerTileId.X;
                int y = closestTileId.Y - centerTileId.Y;

                if (Mathf.Abs(x) > 1 || Mathf.Abs(y) > 1)
                    grid.GenerateGrid((float)location.x, (float)location.y, zoom);
                else
                {

                    grid.ShiftX(x);
                    grid.ShiftY(y);

                }

            }

            player.transform.position = grid.Coordinate_To_Position((float)location.x, (float)location.y);

        }
    }
}
