using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class Branch
{
    public List<TreeVertex> Vertices { get; set; }
    public TreeVertex InterpolatedVertex { get; set; }
    public int Level { get; set; }
    public float SelfGrowthFactor { get; set; }
    
    public Branch(int level)
    {
        Vertices = new List<TreeVertex>();
        InterpolatedVertex = null;
        Level = level;
        SelfGrowthFactor = 0.0f;
    }

    public override string ToString()
    {
        return $"Level: {Level}, Vertices: {Vertices.Count}" +
            $"\n\t{string.Join("\n\t", Vertices)}";
    }


}