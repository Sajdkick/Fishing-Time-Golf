using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{

    public class MapModel
    {

        public int zoom;
        public string mapID;
        public int gridSize;

        public float water_level;

        public GameObject gpsLocationObject;

        public MapGrid grid;

        public ILocationProvider locationProvider;

        MapController mapController;

        public MapModel(MapController _mapController)
        {

            mapController = _mapController;

            //We initialize the locationProvider.
            locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;

            zoom = 17;
            mapID = "mapbox.outdoors";
            gridSize = 5;

            //We initilize the grid.
            Vector2d location = locationProvider.Location;
            grid = new MapGrid((float)location.x, (float)location.y, zoom, mapID, gridSize);

            gpsLocationObject = new GameObject("GPS Location");

        }

        float old_water_level = 0;
        bool gridOutdated = true;
        public void Update()
        {

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

                        if (tile.GetComponent<WaterObjectTile>() == null)
                            tile.gameObject.AddComponent<WaterObjectTile>().Initialize(tile.tileID);

                        FillWater(tile, water_level);

                    }

                    gridOutdated = false;

                }

            }

            if (!Player.player.isCharging())
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

    public class MapView
    {

        MapController mapController;

        public InputField waterLevelText;
        public Text scoreText;
        public Text ballText;
        public Button menuButton;
        public Button leftButton;
        public Button rightButton;

        // Use this for initialization
        public MapView(MapController _mapController)
        {

            mapController = _mapController;

            //We create the gui
            GameObject canvas = GUIManager.CreateCanvas("Canvas");

            Texture2D down = Resources.Load<Texture2D>("UI Elements/Black/2x/left");
            leftButton = GUIManager.CreateButton(canvas, 0.15f, 0.15f, 0.1f, 0.1f, down, "Left Button", (data) => mapController.LowerWaterLevel(data)).GetComponent<Button>();
            Texture2D up = Resources.Load<Texture2D>("UI Elements/Black/2x/right");
            rightButton = GUIManager.CreateButton(canvas, 0.85f, 0.15f, 0.1f, 0.1f, up, "Right Button", (data) => mapController.RiseWaterLevel(data)).GetComponent<Button>();

            Texture2D menu = Resources.Load<Texture2D>("UI Elements/Black/2x/hamburger icon");
            menuButton = GUIManager.CreateButton(canvas, 0.5f, 0.03f, 0.05f, 0.05f, menu, "Menu Button", (data) => ToggleMenu(data)).GetComponent<Button>();

            waterLevelText = GUIManager.CreateInputTextfield(canvas, 0.5f, 0.15f, 0.3f, 0.1f, "Water Level", "0", "LemonMilkbold").GetComponent<InputField>();
            scoreText = GUIManager.CreateText(canvas, 0.5f, 0.85f, 0.1f, 0.2f, Player.player.GetScore().ToString(), "Score", "LemonMilkbold", 15).GetComponent<Text>();
            ballText = GUIManager.CreateText(canvas, 0.25f, 0.85f, 0.05f, 0.05f, Player.player.GetScore().ToString(), "Score", "LemonMilkbold", 15).GetComponent<Text>();

            ToggleMenu(null);

        }

        void ToggleMenu(UnityEngine.EventSystems.PointerEventData data)
        {

            bool toggle = !waterLevelText.gameObject.activeInHierarchy;

            waterLevelText.transform.parent.gameObject.SetActive(toggle);
            leftButton.gameObject.SetActive(toggle);
            rightButton.gameObject.SetActive(toggle);

        }

        public void Update()
        {

            if (Player.player.GetActiveBall() != null)
            {
                ballText.text = "wait";
                ballText.color = Color.red;
            }
            else
            {
                ballText.text = "ready";
                ballText.color = Color.green;
            }

            scoreText.text = Player.player.GetScore().ToString();

        }

    }

    MapModel model;
    MapView view;

    public GameObject player;

    void Start()
    {

        model = new MapModel(this);
        view = new MapView(this);

        LerpPosition lerp = player.AddComponent<LerpPosition>();
        lerp.target = model.gpsLocationObject;
        lerp.t = 0.1f;
        lerp.maxDistance = 0.1f;

    }

    public void LowerWaterLevel(UnityEngine.EventSystems.PointerEventData data)
    {

        model.water_level -= 1 + model.water_level * 0.1f;
        model.water_level = Mathf.Clamp(model.water_level, 0, 1000);
        view.waterLevelText.text = model.water_level.ToString("0");

    }
    public void RiseWaterLevel(UnityEngine.EventSystems.PointerEventData data)
    {

        model.water_level += 1 + model.water_level * 0.1f;
        model.water_level = Mathf.Clamp(model.water_level, 0, 1000);
        view.waterLevelText.text = model.water_level.ToString("0");

    }

    // Update is called once per frame
    void Update()
    {

        int newWaterLevel = 0;
        if (int.TryParse(view.waterLevelText.text, out newWaterLevel))
            model.water_level = newWaterLevel;
        else view.waterLevelText.text = model.water_level.ToString("0");

        model.Update();
        view.Update();

    }

}

