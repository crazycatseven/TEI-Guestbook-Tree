using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;



public class TrunkParser
{
    public static List<TreeVertex> ParseTrunkVertices(string filePath)
        {
            List<TreeVertex> vertices = new List<TreeVertex>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                bool trunkFound = false;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim().StartsWith("Label: Trunk"))
                    {
                        trunkFound = true;
                    }

                    if (trunkFound && line.Trim().StartsWith("Vertex:"))
                    {
                        // Use regex to parse the vertex data
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
                            vertices.Add(vertexData);
                        }
                    }

                    if (trunkFound && line.Trim().StartsWith("Connected Vertices Sequence:"))
                    {
                        string[] sequenceParts = line.Split(':')[1].Trim().Split('-');
                        List<TreeVertex> sortedVertices = new List<TreeVertex>();

                        foreach (string part in sequenceParts)
                        {
                            int index = int.Parse(part.Trim());
                            TreeVertex vertex = vertices.Find(v => v.Index == index);

                            if (vertex != null)
                            {
                                sortedVertices.Add(vertex);
                            }
                        }

                        return sortedVertices;
                    }
                }
            }

            return null;
        }
}