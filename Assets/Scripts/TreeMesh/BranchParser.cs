using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;


public class BranchParser
{
    public static List<Branch> ParseBranches(string filePath, int branchLevel)
    {
        List<Branch> branches = new List<Branch>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            bool branchFound = false;
            bool vertexGroupFound = false;
            Branch currentBranch = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim().StartsWith("Label: Branch" + branchLevel.ToString()))
                {
                    branchFound = true;
                    continue;
                }

                if (branchFound && line.Trim().StartsWith("Vertex Group"))
                {
                    currentBranch = new Branch(branchLevel);  // 重新初始化 currentBranch
                    vertexGroupFound = true;
                    continue;
                }

                if (vertexGroupFound && line.Trim().StartsWith("Vertex:"))
                {
                    // 使用正则表达式解析顶点数据
                    Match vertexMatch = Regex.Match(line, @"Vertex: (\d+), Position: <Vector \((-?\d+\.\d+), (-?\d+\.\d+), (-?\d+\.\d+)\)>, Normal: <Vector \((-?\d+\.\d+), (-?\d+\.\d+), (-?\d+\.\d+)\)>, RadiusX: (-?\d+\.\d+), RadiusY: (-?\d+\.\d+)");

                    if (vertexMatch.Success)
                    {
                        int vertexIndex = int.Parse(vertexMatch.Groups[1].Value);

                        Vector3 position = new Vector3(
                            float.Parse(vertexMatch.Groups[2].Value),
                            float.Parse(vertexMatch.Groups[4].Value),
                            float.Parse(vertexMatch.Groups[3].Value)
                        );

                        Vector3 normal = new Vector3(
                            float.Parse(vertexMatch.Groups[5].Value),
                            float.Parse(vertexMatch.Groups[7].Value),
                            float.Parse(vertexMatch.Groups[6].Value)
                        );

                        float radiusX = float.Parse(vertexMatch.Groups[8].Value);
                        float radiusY = float.Parse(vertexMatch.Groups[9].Value);

                        TreeVertex vertexData = new TreeVertex(vertexIndex, position, normal, radiusX, radiusY);
                        currentBranch.Vertices.Add(vertexData);
                    }
                    continue;
                }

                if (vertexGroupFound && line.Trim().StartsWith("Connected Vertices Sequence:"))
                {
                    string[] sequenceParts = line.Split(':')[1].Trim().Split('-');
                    List<TreeVertex> sortedVertices = new List<TreeVertex>();

                    foreach (string part in sequenceParts)
                    {
                        int index = int.Parse(part.Trim());
                        TreeVertex vertex = currentBranch.Vertices.Find(v => v.Index == index);

                        if (vertex != null)
                        {
                            sortedVertices.Add(vertex);
                        }
                    }

                    currentBranch.Vertices = sortedVertices;
                    branches.Add(currentBranch);
                    vertexGroupFound = false;
                    continue;
                }

                if ((branchFound && !(line.Trim().StartsWith("Label: Branch" + branchLevel.ToString()))) || reader.EndOfStream)
                {
                    branchFound = false;
                }
            }
        }

        return branches;
    }
}
