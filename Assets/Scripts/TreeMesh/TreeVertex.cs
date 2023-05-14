using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeVertex
{
    public int Index { get; }
    public Vector3 Position { get; }
    public Vector3 Normal { get; }
    public float RadiusX { get; }
    public float RadiusY { get; }
    public float RadiusScale { get; set; }
    public float LengthRatio { get; set; }
    public bool IsFork { get; set; }


    public TreeVertex(int index, Vector3 position, Vector3 normal, float radiusX, float radiusY)
    {
        Index = index;
        Position = position;
        Normal = normal;
        RadiusX = radiusX;
        RadiusY = radiusY;
        RadiusScale = 1.0f;
        LengthRatio = -1.0f;
        IsFork = false;
    }

    public override string ToString()
    {
        return $"Index: {Index}, Position: {Position}, Normal: {Normal}, LengthRatio: {LengthRatio}";
    }

}
