using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Leaf
{
    public int Index { get; }
    public Vector3 Position { get; }
    public Vector3 GrowthDirection { get; set;}
    public float StartGlobalGrowthFactor { get; set; }
    public float Scale { get; set; }
    public float RadiusOffset { get; set; }
    public LeafData LeafData { get; set; }
    public bool UpSide { get; set;}


    public Leaf(int index, Vector3 position, float startGlobalGrowthFactor)
    {
        Index = index;
        Position = position;
        GrowthDirection = Vector3.zero;
        StartGlobalGrowthFactor = startGlobalGrowthFactor;
        Scale = 0.2f;
        RadiusOffset = 0.0f;
        LeafData = null;
        UpSide = true;
    }


    public override string ToString()
    {
        return $"Index: {Index}, Position: {Position}, GrowthDirection: {GrowthDirection}, StartGlobalGrowthFactor: {StartGlobalGrowthFactor}";
    }

}
