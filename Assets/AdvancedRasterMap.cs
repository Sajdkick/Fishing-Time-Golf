using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;

public class AdvancedRasterMap : MonoBehaviour
{

    /// <summary>
    /// Used to fetch tiles from mapbox.
    /// </summary>

	void Start()
	{

        //We convert our Lat/Long into coordinates that can be used in UnwrappedTileID.
        Vector2d xyCoords = Conversions.LatitudeLongitudeToTileId(56.1612f, 15.5869f, 16);
        UnwrappedTileId tileID = new UnwrappedTileId(16, (int)xyCoords.x, (int)xyCoords.y);

        new TileFetcher().LoadTile(tileID, "mapbox.dark", TileCallbackFunctions.CreateQuad);

        //new TileFetcher().LoadTile(56.1612f, 15.5869f, 16, "mapbox.terrain-rgb", TileCallbackFunctions.CreateHeightQuad);

    }

}