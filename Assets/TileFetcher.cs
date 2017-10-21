using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;

public class TileFetcher
{

    public delegate void TileCallback(TileFetcher tileFetcher);

    public RawPngRasterTile tile;
    public UnwrappedTileId tileID;
    string mapID;

    /// <summary>
    /// 0: Uninitialized, 1: Initializing, 2: Initialized, -1: Error.
    /// </summary>
    int state = 0;
    public int GetState() { return state; }

    /// <summary>
    /// Can we start using the tile?
    /// </summary>
    public bool isLoaded = false;

    public void LoadTile(UnwrappedTileId _tileID, string _mapID, TileCallback callback)
    {
        
        tile = new RawPngRasterTile();
        tileID = _tileID;
        mapID = _mapID;
        
        state = 1;

        //Here we call the initialize function, this will fetch a tile containing the position provided with the correct zoom.
        tile.Initialize(MapboxAccess.Instance, tileID.Canonical, mapID, () =>
        {
            
            //This function is called upon recieving the tile from map box.

            //We check for errors.
            if (tile.HasError)
            {

                state = -1;
                return;

            }

            state = 2;
            isLoaded = true;

            if (callback != null)
                callback(this);

        });

    }
    public void LoadTile(float latitude, float longitude, int zoom, string mapID, TileCallback callback)
    {

        //We convert our Lat/Long into coordinates that can be used in UnwrappedTileID.
        Vector2d xyCoords = Conversions.LatitudeLongitudeToTileId(latitude, longitude, zoom);
        UnwrappedTileId tileID = new UnwrappedTileId(zoom, (int)xyCoords.x, (int)xyCoords.y);

        LoadTile(tileID, mapID, callback);

    }

}
