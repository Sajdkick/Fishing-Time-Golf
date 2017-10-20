using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;

public class MapGrid : MonoBehaviour {

    public string mapID;
    public float startLatitude;
    public float startLongitude;
    public int gridSize;
    public int zoom;

    int actualGridSize;

    TileObject[,] grid;

    // Use this for initialization
    void Start () {

        actualGridSize = gridSize;
        if (Mathf.Floor(gridSize / 2.0f) == gridSize / 2.0f)
            actualGridSize += 1;

        grid = new TileObject[actualGridSize, actualGridSize];

        //We convert our Lat/Long into coordinates that can be used in UnwrappedTileID.
        Vector2d xyCoords = Conversions.LatitudeLongitudeToTileId(startLatitude, startLongitude, zoom);
        UnwrappedTileId tileID = new UnwrappedTileId(zoom, (int)xyCoords.x, (int)xyCoords.y);

        for(int x = 0; x < actualGridSize; x++)
        {

            for(int y = 0; y < actualGridSize; y++)
            {

                UnwrappedTileId correctID = tileID;
                int xSteps = -Mathf.FloorToInt(actualGridSize / 2.0f) + x;
                int ySteps = -Mathf.FloorToInt(actualGridSize / 2.0f) + y;

                if (xSteps < 0)
                    for (int i = 0; i < Mathf.Abs(xSteps); i++)
                        correctID = correctID.West;
                else
                    for (int i = 0; i < Mathf.Abs(xSteps); i++)
                        correctID = correctID.East;

                if (ySteps < 0)
                    for (int i = 0; i < Mathf.Abs(ySteps); i++)
                        correctID = correctID.South;
                else
                    for (int i = 0; i < Mathf.Abs(ySteps); i++)
                        correctID = correctID.North;

                grid[x, y] = new GameObject(x + ":" + y).AddComponent<TileObject>();
                grid[x, y].Load(correctID, mapID);
                grid[x, y].transform.position = new Vector3(xSteps, ySteps, 0);

            }

        }

    }

    void ShiftX(int x)
    {

        TileObject[] newTiles = new TileObject[actualGridSize];

        int columnEntering = actualGridSize - 1;
        int columnLeaving = 0;
        int direction = -1;

        if (x > 0)
        {

            columnEntering = 0;
            columnLeaving = actualGridSize - 1;
            direction = 1;

        }

        //We load the new tiles that entered the grid.
        for (int j = 0; j < actualGridSize; j++)
        {
            newTiles[j] = new GameObject(grid[columnEntering, j].name).AddComponent<TileObject>();
            newTiles[j].Load(TileIDFromDirection(grid[columnEntering, j].tileID, direction, 0), mapID);
            newTiles[j].transform.position = grid[columnEntering, j].transform.position;
        }

        //We move the tiles and update their name.
        for (int i = columnEntering; i != columnLeaving; i += direction)
            for (int j = 0; j < actualGridSize; j++)
            {
                grid[i, j].transform.position = grid[i + direction, j].transform.position;
                grid[i, j].name = grid[i + direction, j].name;
            }

        //We destroy the tiles that were moved outside of the grid.
        for (int j = 0; j < actualGridSize; j++)
            DestroyImmediate(grid[columnLeaving, j].gameObject);

        //We update the grid position.
        for (int i = columnLeaving; i != columnEntering; i -= direction)
            for (int j = 0; j < actualGridSize; j++)
                grid[i, j] = grid[i - direction, j];

        //We put the new tiles into the grid
        for (int j = 0; j < actualGridSize; j++)
            grid[columnEntering, j] = newTiles[j];

    }

    void ShiftY(int y)
    {

        TileObject[] newTiles = new TileObject[actualGridSize];

        int rowEntering = actualGridSize - 1;
        int rowLeaving = 0;
        int direction = -1;

        if (y < 0)
        {

            rowEntering = 0;
            rowLeaving = actualGridSize - 1;
            direction = 1;

        }

        //We load the new tiles that entered the grid.
        for (int i = 0; i < actualGridSize; i++)
        {
            newTiles[i] = new GameObject(grid[i, rowEntering].name).AddComponent<TileObject>();
            newTiles[i].Load(TileIDFromDirection(grid[i, rowEntering].tileID, 0, direction), mapID);
            newTiles[i].transform.position = grid[i, rowEntering].transform.position;
        }

        //We move the tiles and update their name.
        for (int i = 0; i < actualGridSize; i++)
            for (int j = rowEntering; j != rowLeaving; j += direction)
            {
                grid[i, j].transform.position = grid[i, j + direction].transform.position;
                grid[i, j].name = grid[i, j + direction].name;
            }

        //We destroy the tiles that were moved outside of the grid.
        for (int i = 0; i < actualGridSize; i++)
            DestroyImmediate(grid[i, rowLeaving].gameObject);

        //We update the grid position.
        for (int i = 0; i < actualGridSize; i++)
            for (int j = rowLeaving; j != rowEntering; j -= direction)
                grid[i, j] = grid[i, j - direction];

        //We put the new tiles into the grid
        for (int i = 0; i < actualGridSize; i++)
            grid[i, rowEntering] = newTiles[i];

    }

    UnwrappedTileId TileIDFromDirection(UnwrappedTileId tileID, int x, int y)
    {

        if (x > 0 && y == 0)
            return tileID.West;
        if (x > 0 && y > 0)
            return tileID.SouthWest;
        if (x == 0 && y > 0)
            return tileID.South;
        if (x < 0 && y > 0)
            return tileID.SouthEast;
        if (x < 0 && y == 0)
            return tileID.East;
        if (x < 0 && y < 0)
            return tileID.NorthEast;
        if (x == 0 && y < 0)
            return tileID.North;
        if (x > 0 && y < 0)
            return tileID.NorthWest;

        return tileID;

    }

    // Update is called once per frame
    void Update () {

        if (Input.GetKeyDown(KeyCode.Space))
            ShiftY(1);

	}
}
