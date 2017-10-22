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

    public float water_level;

    public GameObject player;
    GameObject gpsLocationObject;

    MapGrid grid;

    ILocationProvider locationProvider;

	// Use this for initialization
	void Start () {

        //We initialize the locationProvider.
        locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;

        //We initilize the grid.
        Vector2d location = locationProvider.Location;
        grid = new MapGrid((float)location.x, (float)location.y, zoom, mapID, gridSize);

        gpsLocationObject = new GameObject("GPS Location");
        LerpPosition lerp = player.AddComponent<LerpPosition>();
        lerp.target = gpsLocationObject;
        lerp.t = 0.1f;
        lerp.maxDistance = 10;

        //We create the gui
        GameObject canvas = GUIManager.CreateCanvas("Canvas");

        Texture2D down = Resources.Load<Texture2D>("UI Elements/Black/2x/down");
        GUIManager.CreateButton(canvas, 0.1f, 0.25f, 0.1f, 0.1f, down, "Down Button", (data) => LowerWaterLevel(data));
        Texture2D up = Resources.Load<Texture2D>("UI Elements/Black/2x/up");
        GUIManager.CreateButton(canvas, 0.1f, 0.55f, 0.1f, 0.1f, up, "Down Up", (data) => RiseWaterLevel(data));

    }

    void LowerWaterLevel(UnityEngine.EventSystems.PointerEventData data)
    {

        water_level -= 1 + water_level * 0.1f;
        water_level = Mathf.Clamp(water_level, 0, 1000);

    }
    void RiseWaterLevel(UnityEngine.EventSystems.PointerEventData data)
    {

        water_level += 1 + water_level * 0.1f;
        water_level = Mathf.Clamp(water_level, 0, 1000);

    }

    float old_water_level = 0;
    bool gridOutdated = false;
	// Update is called once per frame
	void Update () {

        Vector2d location = locationProvider.Location;

        if (grid.UpdateGrid((float)location.x, (float)location.y))
            gridOutdated = true;

        if (grid.AllTilesLoaded())
        {

            if (gridOutdated || old_water_level != water_level)
            {

                TileObject[,] allTiles = grid.GetAllTiles();
                foreach (TileObject tile in allTiles)
                {

                    FillWater(tile, water_level);

                }

                gridOutdated = false;

            }

        }

        gpsLocationObject.transform.position = grid.Coordinate_To_Position((float)location.x, (float)location.y);
        old_water_level = water_level;
        
    }

    void FillWater(TileObject tile, float waterLevel)
    {

        //We convert to a mat
        Mat mapMat = new Mat(new Size(tile.DisplayTexture.width, tile.DisplayTexture.height), CvType.CV_8UC3);
        Utils.texture2DToMat(tile.originalMapTexture, mapMat);

        Mat waterRegion = new Mat();
        Core.compare(tile.heightMap, new Scalar(waterLevel), waterRegion, Core.CMP_LT);

        mapMat.setTo(new Scalar(0, 0, 255), waterRegion);

        //we convert it into a texture.
        tile.DisplayTexture.Resize(mapMat.width(), mapMat.height());
        Utils.matToTexture2D(mapMat, tile.DisplayTexture);

        Vector3 tileCorner = tile.transform.position - Vector3.right * 0.5f - Vector3.up * 0.5f;
        Random.InitState(tile.tileID.GetHashCode());

        //We remove all the obstacles.
        for(int i = 0; i < tile.tileQuad.transform.childCount; i++)
            Destroy(tile.tileQuad.transform.GetChild(i).gameObject);

        Core.transpose(mapMat, mapMat);
        //We generate all the obstacles.
        for(int i = 0; i < 20; i++)
        {

            int x = Random.Range(0, 255);
            int y = Random.Range(0, 255);

            byte[] matElement = new byte[3];
            mapMat.get(x, y, matElement);
            if(matElement[0] == 0 && matElement[1] == 0 && matElement[2] == 255)
            {

                SpawnObstacle(tile, tileCorner + new Vector3(x / 255.0f, 1 - (y / 255.0f), 0));

            }

        }

        mapMat.release();
        waterRegion.release();

    }

    void SpawnObstacle(TileObject tile, Vector3 position)
    {

        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obstacle.transform.position = position;
        obstacle.transform.localScale *= 0.05f;
        obstacle.transform.parent = tile.tileQuad.transform;

        Rigidbody rigidbody = obstacle.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

    }

}
