using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeafUtils
{

    public static float InterpolationThreshold = 0.3f;
    public static List<Leaf> GetLeaves(List<Branch> branches)
    {
        // Set random seed
        Random.InitState(42);
        List<Leaf> leaves = new List<Leaf>();
        foreach (Branch branch in branches)
        {
            for (int i = 0; i < branch.Vertices.Count; i++)
            {

                if (branch.Vertices[i].IsFork != true)
                {
                    Leaf leaf = new Leaf(i, branch.Vertices[i].Position, branch.StartGlobalGrowthFactors[i]);

                     // Calculate leaf growth direction
                    Vector3 branchDirection = Vector3.zero;
                    if (i < branch.Vertices.Count - 1)
                    {
                        branchDirection = branch.Vertices[i + 1].Position - branch.Vertices[i].Position;
                    }
                    else if (branch.SubBranches.Count > 0)
                    {
                        branchDirection = branch.SubBranches[0].Vertices[0].Position - branch.Vertices[i].Position;

                    }


                    leaf.GrowthDirection = branchDirection;
                    


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
                        interpolatedLeaf.GrowthDirection = branch.Vertices[i + 1].Position - branch.Vertices[i].Position;

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

                        // Calculate leaf growth direction
                        Vector3 branchDirection = Vector3.zero;
                        if (i < subBranch.Vertices.Count - 1)
                        {
                            branchDirection = subBranch.Vertices[i + 1].Position - subBranch.Vertices[i].Position;
                        }
                        else if (subBranch.SubBranches.Count > 0)
                        {
                            branchDirection = subBranch.SubBranches[0].Vertices[0].Position - subBranch.Vertices[i].Position;
                        }
                        leaf.GrowthDirection = branchDirection;

                        leaves.Add(leaf);

                        // If the distance between two vertices is greater than the threshold, interpolate between them

                        if (i < subBranch.Vertices.Count - 1 && 
                        Vector3.Distance(subBranch.Vertices[i].Position, subBranch.Vertices[i + 1].Position) > InterpolationThreshold)
                        {
                            float t = Random.Range(0.2f, 0.8f);
                            Vector3 interpolatedPosition = Vector3.Lerp(subBranch.Vertices[i].Position, subBranch.Vertices[i + 1].Position, t);
                            Leaf interpolatedLeaf = new Leaf(i, interpolatedPosition, Mathf.Lerp(subBranch.StartGlobalGrowthFactors[i], subBranch.StartGlobalGrowthFactors[i+1], t));
                            interpolatedLeaf.GrowthDirection = branch.Vertices[i + 1].Position - branch.Vertices[i].Position;
                            leaves.Add(interpolatedLeaf);
                        }

                    }
                }
            }
        }

        leaves.Sort((a, b) => a.StartGlobalGrowthFactor.CompareTo(b.StartGlobalGrowthFactor));

        return leaves;
    }

    private static Vector3 CalculateLeafGrowthDirection(Vector3 branchDirection)
    {
        // // Calculate the perpendicular vector to the branch direction
        // Vector3 perpendicular = Vector3.Cross(branchDirection, Vector3.up).normalized;

        // // Calculate the projected direction in the branch's plane
        // Vector3 projectedDirection = Vector3.Cross(branchDirection, perpendicular).normalized;

        // return projectedDirection;

        return branchDirection;
    }


    public static int GetAvailableLeavesNumber(List<Leaf> leaves, float currentGrowthFactor)
    {
        int leavesNumber = 0;
        foreach (Leaf leaf in leaves)
        {
            if (leaf.StartGlobalGrowthFactor <= currentGrowthFactor)
            {
                leavesNumber++;
            }
        }
        return leavesNumber;
    }

    public static Vector3 GetNextAvailableLeafPosition(List<Leaf> leaves, float currentGrowthFactor)
    {
        foreach (Leaf leaf in leaves)
        {
            if (leaf.StartGlobalGrowthFactor > currentGrowthFactor)
            {
                return leaf.Position;
            }
        }
        return Vector3.zero;
    }



}