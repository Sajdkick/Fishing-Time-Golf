﻿using UnityEngine;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using OpenCVForUnity;

public class TileCallbackFunctions : MonoBehaviour {

    static public void CreateQuad(TileFetcher tileFetcher)
    {

        //This function is called upon recieving the tile from map box.

        //We check for errors.
        if (tileFetcher.tile.HasError)
            return;

        //We create the quad.
        var tileQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tileQuad.name = tileFetcher.tileID.Canonical.ToString();
        tileQuad.transform.position = new Vector3(0, 0, 0);

        //We create the texture.
        var texture = new Texture2D(0, 0);
        texture.LoadImage(tileFetcher.tile.Data);

        //We assign it to the quad.
        var material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = texture;
        tileQuad.GetComponent<MeshRenderer>().sharedMaterial = material;

    }
    static public void CreateHeightQuad(TileFetcher tileFetcher)
    {

        //This function is called upon recieving the tile from map box.

        //We check for errors.
        if (tileFetcher.tile.HasError)
            return;

        //We create the quad.
        var tileQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tileQuad.name = tileFetcher.tileID.Canonical.ToString();
        tileQuad.transform.position = new Vector3(0, 0, 0);

        //We create the texture.
        var texture = new Texture2D(0, 0);
        texture.LoadImage(tileFetcher.tile.Data);
        texture = HeightMat_To_Texture2D(TerrainRGB_To_HeightMat(texture));

        //We assign it to the quad.
        var material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = texture;
        tileQuad.GetComponent<MeshRenderer>().sharedMaterial = material;

    }

    /// <summary>
    /// Converts what we get from mapbox.terrain-rgb into a mat containing the height values.
    /// </summary>
    /// <param name="rgbTexture">A texture containing the terrain-rgb data</param>
    /// <returns>A Mat of the type 32SC1.</returns>
    static public Mat TerrainRGB_To_HeightMat(Texture2D rgbTexture)
    {

        //We convert to a mat
        Mat rgbMat = new Mat(new Size(rgbTexture.width, rgbTexture.height), CvType.CV_8UC3);
        Utils.texture2DToMat(rgbTexture, rgbMat);

        //We split so we get each channel as a mat.
        List<Mat> channelList = new List<Mat>();
        Core.split(rgbMat, channelList);

        //We convert to a higher resolution type.
        channelList[0].convertTo(channelList[0], CvType.CV_32SC1);
        channelList[1].convertTo(channelList[1], CvType.CV_32SC1);
        channelList[2].convertTo(channelList[2], CvType.CV_32SC1);

        //We follow this algorithm: height = -10000 + ((R * 256 * 256 + G * 256 + B) * 0.1)
        Mat R = new Mat();
        Core.multiply(channelList[0], new Scalar(256 * 256), R);
        Mat G = new Mat();
        Core.multiply(channelList[1], new Scalar(256), G);
        Mat B = channelList[2];

        Mat heightMat = new Mat(new Size(rgbTexture.width, rgbTexture.height), CvType.CV_32SC1);
        Core.add(R, G, heightMat);
        Core.add(heightMat, B, heightMat);
        Core.multiply(heightMat, new Scalar(0.1f), heightMat);
        Core.add(heightMat, new Scalar(-10000), heightMat);

        return heightMat;

    }

    /// <summary>
    /// Takes a Mat of the type 32SC1 and turns it into a texture.
    /// </summary>
    /// <param name="heightMat">A mat in the format 32SC1, you get it from TerrainRGB_To_HeightMat</param>
    /// <returns></returns>
    static public Texture2D HeightMat_To_Texture2D(Mat heightMat)
    {

        //To make it into a texture it needs to be 8 bit.
        heightMat.convertTo(heightMat, CvType.CV_8UC1);

        //we convert it into a texture.
        Texture2D heightTexture = new Texture2D(heightMat.width(), heightMat.height());
        Utils.matToTexture2D(heightMat, heightTexture);
        return heightTexture;

    }

}