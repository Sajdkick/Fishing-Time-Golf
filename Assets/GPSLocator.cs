using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;
using UnityEngine.UI;

public class GPSLocator : MonoBehaviour {

    public int zoom;
    public string mapID;
    public int gridSize;

    public float water_level;

    public GameObject player;
    GameObject gpsLocationObject;

    MapGrid grid;

    ILocationProvider locationProvider;

    InputField waterLevelText;
    Text scoreText;
    Text ballText;
    Button menuButton;
    Button leftButton;
    Button rightButton;


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
        lerp.maxDistance = 0.1f;

        //We create the gui
        GameObject canvas = GUIManager.CreateCanvas("Canvas");

        Texture2D down = Resources.Load<Texture2D>("UI Elements/Black/2x/left");
        leftButton = GUIManager.CreateButton(canvas, 0.15f, 0.15f, 0.1f, 0.1f, down, "Left Button", (data) => LowerWaterLevel(data)).GetComponent<Button>();
        Texture2D up = Resources.Load<Texture2D>("UI Elements/Black/2x/right");
        rightButton = GUIManager.CreateButton(canvas, 0.85f, 0.15f, 0.1f, 0.1f, up, "Right Button", (data) => RiseWaterLevel(data)).GetComponent<Button>();

        Texture2D menu = Resources.Load<Texture2D>("UI Elements/Black/2x/hamburger icon");
        menuButton = GUIManager.CreateButton(canvas, 0.5f, 0.03f, 0.05f, 0.05f, menu, "Menu Button", (data) => ToggleMenu(data)).GetComponent<Button>();

        waterLevelText = GUIManager.CreateInputTextfield(canvas, 0.5f, 0.15f, 0.3f, 0.1f, "Water Level", water_level.ToString("0.00"), "LemonMilkbold").GetComponent<InputField>();
        scoreText = GUIManager.CreateText(canvas, 0.5f, 0.85f, 0.1f, 0.2f, Player.GetScore().ToString(), "Score", "LemonMilkbold", 15).GetComponent<Text>();
        ballText = GUIManager.CreateText(canvas, 0.25f, 0.85f, 0.075f, 0.075f, Player.GetScore().ToString(), "Score", "LemonMilkbold", 15).GetComponent<Text>();

        ToggleMenu(null);

    }

    void LowerWaterLevel(UnityEngine.EventSystems.PointerEventData data)
    {

        water_level -= 1 + water_level * 0.1f;
        water_level = Mathf.Clamp(water_level, 0, 1000);
        waterLevelText.text = water_level.ToString("0");

    }
    void RiseWaterLevel(UnityEngine.EventSystems.PointerEventData data)
    {

        water_level += 1 + water_level * 0.1f;
        water_level = Mathf.Clamp(water_level, 0, 1000);
        waterLevelText.text = water_level.ToString("0");

    }
    void ToggleMenu(UnityEngine.EventSystems.PointerEventData data)
    {

        bool toggle = !waterLevelText.gameObject.activeInHierarchy;

        waterLevelText.transform.parent.gameObject.SetActive(toggle);
        leftButton.gameObject.SetActive(toggle);
        rightButton.gameObject.SetActive(toggle);

    }
    float old_water_level = 0;
    bool gridOutdated = true;
	// Update is called once per frame
	void Update () {

        int newWaterLevel = 0;
        if (int.TryParse(waterLevelText.text, out newWaterLevel))
            water_level = newWaterLevel;
        else waterLevelText.text = water_level.ToString("0");

        if (Player.GetActiveBall() != null)
        {
            ballText.text = "wait";
            ballText.color = Color.red;   
        }
        else
        {
            ballText.text = "ready";
            ballText.color = Color.green;
        }

        scoreText.text = Player.GetScore().ToString();

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

                    if(tile.GetComponent<WaterObjectTile>() == null)
                        tile.gameObject.AddComponent<WaterObjectTile>().Initialize(tile.tileID);

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
        List<Mat> channels = new List<Mat>();
        Utils.texture2DToMat(tile.originalMapTexture, mapMat);
        Core.split(mapMat, channels);
        Mat water = new Mat();
        Mat B_GT_R = new Mat();
        Core.compare(channels[2], channels[0], B_GT_R, Core.CMP_GT);
        Mat B_GT_G = new Mat();
        Core.compare(channels[2], channels[1], B_GT_G, Core.CMP_GE);
        Core.multiply(B_GT_R, B_GT_G, water);

        if (waterLevel == 0)
        {

            mapMat.setTo(new Scalar(0, 0, 255), water);

            //we convert it into a texture.
            tile.DisplayTexture.Resize(mapMat.width(), mapMat.height());
            Utils.matToTexture2D(mapMat, tile.DisplayTexture);

            water.release();

        }
        else
        {

            Mat tempHeight = new Mat(tile.heightMap.size(), tile.heightMap.type());
            tile.heightMap.copyTo(tempHeight);
            Mat noise = new Mat(tempHeight.size(), tempHeight.type());
            Core.setRNGSeed(tile.GetHashCode());
            Core.randn(noise, 2f, 1f);
            tempHeight += noise;

            Mat waterRegion = new Mat();
            Core.compare(tempHeight, new Scalar(waterLevel), waterRegion, Core.CMP_LE);

            Imgproc.pyrDown(waterRegion, waterRegion);
            Imgproc.pyrDown(waterRegion, waterRegion);
            Imgproc.pyrDown(waterRegion, waterRegion);
            Imgproc.pyrUp(waterRegion, waterRegion);
            Imgproc.pyrUp(waterRegion, waterRegion);
            Imgproc.pyrUp(waterRegion, waterRegion);

            Imgproc.GaussianBlur(waterRegion, waterRegion, new Size(3, 3), 2);
            waterRegion += water;

            mapMat.setTo(new Scalar(0, 0, 255), waterRegion);

            //we convert it into a texture.
            tile.DisplayTexture.Resize(mapMat.width(), mapMat.height());
            Utils.matToTexture2D(mapMat, tile.DisplayTexture);

            waterRegion.release();

        }

        Core.transpose(mapMat, mapMat);

        WaterObjectTile waterObjectTile = tile.gameObject.GetComponent<WaterObjectTile>();
        //We generate all the obstacles.
        for (int i = 0; i < waterObjectTile.waterObjects.Count; i++)
        {

            int x = waterObjectTile.waterObjects[i].x;
            int y = waterObjectTile.waterObjects[i].y;

            byte[] matElement = new byte[3];
            mapMat.get(x, y, matElement);
            if (matElement[0] == 0 && matElement[1] == 0 && matElement[2] == 255)
            {

                waterObjectTile.waterObjects[i].gameObject.SetActive(true);

            }
            else
            {

                waterObjectTile.waterObjects[i].gameObject.SetActive(false);

            }

        }

        mapMat.release();

    }

}
