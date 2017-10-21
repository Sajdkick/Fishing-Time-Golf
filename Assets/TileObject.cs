using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;

public class TileObject : MonoBehaviour {

    public Texture2D mapTexture;
    public UnwrappedTileId tileID;
    public int state;

	// Use this for initialization
	void Start () {

        state = 0;

	}

    public void Load(UnwrappedTileId tileID, string mapID)
    {

        state = 1;
        new TileFetcher().LoadTile(tileID, mapID, CreateTileQuad);

    }

    public void CreateTileQuad(TileFetcher tileFetcher)
    {

        //This function is called upon recieving the tile from map box.

        //We check for errors.
        if (tileFetcher.tile.HasError)
        {
            state = -1;
            return;
        }

        //We create the quad.
        var tileQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tileQuad.name = tileFetcher.tileID.Canonical.ToString();
        tileQuad.transform.parent = transform;
        tileQuad.transform.localPosition = new Vector3(0, 0, 0);

        //We create the texture.
        var texture = new Texture2D(0, 0);
        texture.LoadImage(tileFetcher.tile.Data);

        //We assign it to the quad.
        var material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = texture;
        tileQuad.GetComponent<MeshRenderer>().sharedMaterial = material;

        mapTexture = texture;
        tileID = tileFetcher.tileID;

        state = 2;

    }
    public void Destroy()
    {

        DestroyImmediate(gameObject);

    }
}
