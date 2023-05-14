using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeafUtils
{

    public static float InterpolationThreshold = 0.3f;
    public static List<Leaf> GetLeaves(List<Branch> branches)
    {
        List<Leaf> leaves = new List<Leaf>();
        foreach (Branch branch in branches)
        {
            for (int i = 0; i < branch.Vertices.Count; i++)
            {

                if (branch.Vertices[i].IsFork != true)
                {
                    Leaf leaf = new Leaf(i, branch.Vertices[i].Position, branch.StartGlobalGrowthFactors[i]);
                    leaves.Add(leaf);
                }

                // Randomly interpolate between two vertices
                if (i < branch.Vertices.Count - 1)
                {
                    // If the distance between two vertices is greater than the threshold, interpolate between them
                    if (Vector3.Distance(branch.Vertices[i].Position, branch.Vertices[i + 1].Position) > InterpolationThreshold)
                    {
                        float t = Random.Range(0.2f, 0.8f);
                        Vector3 interpolatedPosition = Vector3.Lerp(branch.Vertices[i].Position, branch.Vertices[i + 1].Position, t);
                        Leaf interpolatedLeaf = new Leaf(i, interpolatedPosition, Mathf.Lerp(branch.StartGlobalGrowthFactors[i], branch.StartGlobalGrowthFactors[i+1], t));
                        leaves.Add(interpolatedLeaf);
                    }
                }



            }

            foreach (Branch subBranch in branch.SubBranches)
            {
                for (int i = 0; i < subBranch.Vertices.Count; i++)
                {
                    if (subBranch.Vertices[i].IsFork != true)
                    {
                        Leaf leaf = new Leaf(i, subBranch.Vertices[i].Position, subBranch.StartGlobalGrowthFactors[i]);
                        leaves.Add(leaf);

                        // If the distance between two vertices is greater than the threshold, interpolate between them

                        if (i < subBranch.Vertices.Count - 1 && 
                        Vector3.Distance(subBranch.Vertices[i].Position, subBranch.Vertices[i + 1].Position) > InterpolationThreshold)
                        {
                            float t = Random.Range(0.2f, 0.8f);
                            Vector3 interpolatedPosition = Vector3.Lerp(subBranch.Vertices[i].Position, subBranch.Vertices[i + 1].Position, t);
                            Leaf interpolatedLeaf = new Leaf(i, interpolatedPosition, Mathf.Lerp(subBranch.StartGlobalGrowthFactors[i], subBranch.StartGlobalGrowthFactors[i+1], t));
                            leaves.Add(interpolatedLeaf);
                        }

                    }
                }
            }
        }

        Debug.Log($"Leaves: {leaves.Count}");

        return leaves;
    }

}