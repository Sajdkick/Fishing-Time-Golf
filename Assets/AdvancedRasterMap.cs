using UnityEngine;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Utils;

public class AdvancedRasterMap : MonoBehaviour
{
	void Start()
	{

		var pngRasterTile = new RawPngRasterTile();

		//16, 10473, 25333 This works, looks like the top corner of africa?
		UnwrappedTileId tile = new UnwrappedTileId (16, (int)(200 * 1f), (int)(300 * 1f));
		var mapID = "mapbox.dark";

		pngRasterTile.Initialize(MapboxAccess.Instance, tile.Canonical, mapID, () =>
			{
				if (pngRasterTile.HasError)
					return;

				var tileQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
				tileQuad.transform.SetParent(transform);
				tileQuad.name = tile.Canonical.ToString();
				tileQuad.transform.position = new Vector3(0, 0, 0);
				var texture = new Texture2D(0, 0);
				texture.LoadImage(pngRasterTile.Data);
				var material = new Material(Shader.Find("Unlit/Texture"));
				material.mainTexture = texture;
				tileQuad.GetComponent<MeshRenderer>().sharedMaterial = material;
			});

	}
}