using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Leaf
{
    public int Index { get; }
    public Vector3 Position { get; }
    public Vector3 Normal { get; }
    public float StartGlobalGrowthFactor { get; set; }
    public float Scale { get; set; }



    public Leaf(int index, Vector3 position, float startGlobalGrowthFactor)
    {
        Index = index;
        Position = position;
        Normal = Vector3.zero;
        StartGlobalGrowthFactor = startGlobalGrowthFactor;
        Scale = 1.0f;
    }



    public override string ToString()
    {
        return $"Index: {Index}, Position: {Position}, Normal: {Normal}, StartGlobalGrowthFactor: {StartGlobalGrowthFactor}";
    }

}
