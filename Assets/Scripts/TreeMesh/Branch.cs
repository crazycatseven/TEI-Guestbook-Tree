using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class Branch
{
    public List<TreeVertex> Vertices { get; set; }
    public List<Branch> SubBranches { get; set; }
    public TreeVertex InterpolatedVertex { get; set; }
    public int Level { get; set; }
    public float[] StartGlobalGrowthFactors { get; set; }
    public float SelfGrowthFactor { get; set; }
    public float[] LengthRatios { get; set; }
    public float MinRadiusFactor;
    public float MaxRadiusFactor;

    
    public Branch(int level)
    {
        Vertices = new List<TreeVertex>();
        SubBranches = new List<Branch>();
        InterpolatedVertex = null;
        Level = level;
        StartGlobalGrowthFactors = new float[0];
        SelfGrowthFactor = 0.0f;
        LengthRatios = new float[0];
        MinRadiusFactor = 0.5f;
        MaxRadiusFactor = 1.25f;
    }

    public override string ToString()
    {
        return $"Level: {Level}, Vertices: {Vertices.Count}" +
            $"\n\t{string.Join("\n\t", Vertices)}";
    }


}