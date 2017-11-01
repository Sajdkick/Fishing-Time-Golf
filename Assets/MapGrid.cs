using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;

public class MapGrid {

    public string mapID;

    int actualGridSize;
    int zoom;
    bool initialized = false;

    TileObject[,] grid;

    public MapGrid(float latitude, float longitude, int _zoom, string _mapID, int _size)
    {

        zoom = _zoom;
        mapID = _mapID;

        actualGridSize = _size;
        if (Mathf.Floor(_size / 2.0f) == _size / 2.0f)
            actualGridSize += 1;

        grid = new TileObject[actualGridSize, actualGridSize];

        GenerateGrid(latitude, longitude, zoom);

    }
    public MapGrid(Vector2d coordinates, int _zoom, string _mapID, int _size) : this((float)coordinates.x, (float)coordinates.y, _zoom, _mapID, _size) { }

    public bool UpdateGrid(float latitude, float longitude)
    {

        if (AllTilesLoaded())
        {

            UnwrappedTileId closestTileId = GetClosestTile(latitude, longitude).tileID;
            UnwrappedTileId centerTileId = GetCenterTile().tileID;

            if (closestTileId.X != centerTileId.X || closestTileId.Y != centerTileId.Y)
            {

                int x = closestTileId.X - centerTileId.X;
                int y = closestTileId.Y - centerTileId.Y;

                if (Mathf.Abs(x) > 1 || Mathf.Abs(y) > 1)
                    GenerateGrid(latitude, longitude, zoom);
                else
                {

                    ShiftX(x);
                    ShiftY(y);

                }

                return true;

            }

        }

        return false;

    }
    public void GenerateGrid(float latitude, float longitude, int _zoom)
    {

        if (TilesLoading() != 0)
            return;

        //We clear the grid.
        foreach (TileObject tileObject in grid)
            if(tileObject != null)
                tileObject.Destroy();

        zoom = _zoom;

        //We convert our Lat/Long into coordinates that can be used in UnwrappedTileID.
        Vector2d xyCoords = Conversions.LatitudeLongitudeToTileId(latitude, longitude, zoom);
        UnwrappedTileId tileID = new UnwrappedTileId(zoom, (int)xyCoords.x, (int)xyCoords.y);

        for (int x = 0; x < actualGridSize; x++)
        {

            for (int y = 0; y < actualGridSize; y++)
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

        initialized = true;

    }

    public bool AllTilesLoaded()
    {

        if (!initialized)
            return false;

        foreach(TileObject tileObject in grid)
        {

            if (tileObject.state != 2)
                return false;

        }

        return true;

    }
    public int TilesLoading()
    {

        if (!initialized)
            return 0;

        int counter = 0;
        foreach (TileObject tileObject in grid)
        {

            if (tileObject.state == 1)
                counter++;

        }

        return counter;

    }
    public TileObject GetCenterTile()
    {
        return grid[Mathf.FloorToInt(actualGridSize / 2.0f), Mathf.FloorToInt(actualGridSize / 2.0f)];
    }
    public TileObject GetTileFromCenterOffset(int x, int y)
    {
        return grid[Mathf.FloorToInt(actualGridSize / 2.0f) + x, Mathf.FloorToInt(actualGridSize / 2.0f) + y];
    }
    public TileObject GetClosestTile(Vector2d coordinates)
    {

        return GetClosestTile((float)coordinates.x, (float)coordinates.y);

    }
    public TileObject GetClosestTile(float latitude, float longitude)
    {

        Vector2d targetCoords = new Vector2d(latitude, longitude);

        TileObject closestTile = grid[0, 0];
        double minDistance = double.MaxValue;
        foreach (TileObject tile in grid)
        {

            Vector2d coords = Conversions.TileIdToCenterLatitudeLongitude(tile.tileID.X, tile.tileID.Y, tile.tileID.Z);
            if ((targetCoords - coords).magnitude < minDistance)
            {
                closestTile = tile;
                minDistance = (targetCoords - coords).magnitude;
            }

        }

        return closestTile;

    }
    public TileObject GetClosestTileFromPosition(Vector3 position)
    {

        TileObject closestTile = grid[0, 0];
        double minDistance = double.MaxValue;
        foreach (TileObject tile in grid)
        {

            Vector3 tilePosition = tile.transform.position;
            if ((position - tilePosition).magnitude < minDistance)
            {
                closestTile = tile;
                minDistance = (position - tilePosition).magnitude;
            }

        }

        return closestTile;

    }
    public TileObject[,] GetAllTiles()
    {
        return grid;
    }

    public Vector3 Coordinate_To_Position(Vector2d coordinates)
    {

        return Coordinate_To_Position((float)coordinates.x, (float)coordinates.y);

    }
    public Vector3 Coordinate_To_Position(float latitude, float longitude)
    {

        TileObject closestTile = GetClosestTile(latitude, longitude);
        UnwrappedTileId closestTileId = closestTile.tileID;
        Vector2d centerCoords = Conversions.TileIdToCenterLatitudeLongitude(closestTileId.X, closestTileId.Y, closestTileId.Z);
        Vector2d meterOffset = Conversions.LatLonToMeters(new Vector2d(latitude, longitude)) - Conversions.LatLonToMeters(centerCoords);

        float tileScale = Conversions.GetTileScaleInMeters(latitude, zoom);
        Vector2d pixelOffset = meterOffset / tileScale;
        Vector2d worldOffset = pixelOffset / 256.0f;

        return closestTile.transform.position + new Vector3((float)worldOffset.x / 2, (float)worldOffset.y / 2, 0);

    }
    public Vector2d Position_To_Coordinate(Vector3 position)
    {

        TileObject closestTile = GetClosestTileFromPosition(position);
        UnwrappedTileId closestTileId = closestTile.tileID;
        Vector2d centerCoords = Conversions.TileIdToCenterLatitudeLongitude(closestTileId.X, closestTileId.Y, closestTileId.Z);
        Vector3 unityOffset = (position - closestTile.transform.position);
        Vector2 pixelOffset = new Vector2(unityOffset.x * 256, unityOffset.y * 256);

        float tileScale = Conversions.GetTileScaleInMeters((float)centerCoords.x, zoom);
        Vector2d meterOffset = new Vector2d(pixelOffset.x * tileScale, pixelOffset.y * tileScale);

        return Conversions.MetersToLatLon(Conversions.LatLonToMeters(centerCoords) + meterOffset);

    }

    public void ShiftX(int x)
    {

        if (x == 0 || !AllTilesLoaded())
            return;

        TileObject[] newTiles = new TileObject[actualGridSize];

        int columnEntering = actualGridSize - 1;
        int columnLeaving = 0;
        int direction = -1;

        if (x < 0)
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
            grid[columnLeaving, j].Destroy();

        //We update the grid position.
        for (int i = columnLeaving; i != columnEntering; i -= direction)
            for (int j = 0; j < actualGridSize; j++)
                grid[i, j] = grid[i - direction, j];

        //We put the new tiles into the grid
        for (int j = 0; j < actualGridSize; j++)
            grid[columnEntering, j] = newTiles[j];

    }
    public void ShiftY(int y)
    {

        if (y == 0 || !AllTilesLoaded())
            return;

        TileObject[] newTiles = new TileObject[actualGridSize];

        int rowEntering = actualGridSize - 1;
        int rowLeaving = 0;
        int direction = -1;

        if (y > 0)
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
            grid[i, rowLeaving].Destroy();

        //We update the grid position.
        for (int i = 0; i < actualGridSize; i++)
            for (int j = rowLeaving; j != rowEntering; j -= direction)
                grid[i, j] = grid[i, j - direction];

        //We put the new tiles into the grid
        for (int i = 0; i < actualGridSize; i++)
            grid[i, rowEntering] = newTiles[i];

    }

    public UnwrappedTileId TileIDFromDirection(UnwrappedTileId tileID, int x, int y)
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

}
