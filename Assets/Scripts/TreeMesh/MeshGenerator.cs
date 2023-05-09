using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator
{
    public static Mesh CreateTrunkMesh(List<TreeVertex> trunkVerticesList, int radialSegments, int actualTrunkVertexCount)
    {
        Vector3[] trunkVertices = trunkVerticesList.ConvertAll(vertex => vertex.Position).ToArray();

        // 方法实现

        bool interpolation = actualTrunkVertexCount < trunkVerticesList.Count - 1;
        int interpolationVertexIndex = trunkVerticesList.Count - 1;

        // Step 1: Create a new Mesh instance
        Mesh trunkMesh = new Mesh();

        // Step 2: Create a ring for each vertex and store them in a vertex array
        // int vertexCount = trunkVertices.Length * (radialSegments + 1);
        int vertexCount = (actualTrunkVertexCount + (interpolation ? 1 : 0)) * (radialSegments + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];


        // Debug.Log("actualTrunkVertexCount: " + actualTrunkVertexCount);

        for (int i = 0; i < actualTrunkVertexCount; i++)
        {
            float trunkRadius = trunkVerticesList[i].RadiusX * trunkVerticesList[i].RadiusScale;

            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = 2 * Mathf.PI * j / radialSegments;
                float x = trunkRadius * Mathf.Cos(angle);
                float z = trunkRadius * Mathf.Sin(angle);

                Vector3 vertexOffset = new Vector3(x, 0, z);
                vertices[i * (radialSegments + 1) + j] = trunkVertices[i] + vertexOffset;
                uv[i * (radialSegments + 1) + j] = new Vector2((float)j / radialSegments, (float)i / (actualTrunkVertexCount - 1));
            }

            if (interpolation && i == actualTrunkVertexCount - 1)
            {
                for (int j = 0; j <= radialSegments; j++)
                {
                    trunkRadius = trunkVerticesList[interpolationVertexIndex].RadiusX * trunkVerticesList[interpolationVertexIndex].RadiusScale;
                    float angle = 2 * Mathf.PI * j / radialSegments;
                    float x = trunkRadius * Mathf.Cos(angle);
                    float z = trunkRadius * Mathf.Sin(angle);

                    Vector3 vertexOffset = new Vector3(x, 0, z);
                    vertices[(i+1) * (radialSegments + 1) + j] = trunkVertices[interpolationVertexIndex] + vertexOffset;
                    uv[(i+1) * (radialSegments + 1) + j] = new Vector2((float)j / radialSegments, (float)(i+1) / (actualTrunkVertexCount - 1));
                }
            }
        }

        // Step 3: Create triangles based on the vertex array and store them in a triangle array
        int[] triangles = new int[(actualTrunkVertexCount + (interpolation ? 0 : -1)) * radialSegments * 6];
        // for (int i = 0; i < actualTrunkVertexCount - 1; i++)
        // {
        //     for (int j = 0; j < radialSegments; j++)
        //     {
        //         int baseIndex = i * (radialSegments + 1) + j;
        //         int triangleIndex = (i * radialSegments + j) * 6;

        //         triangles[triangleIndex] = baseIndex;
        //         triangles[triangleIndex + 1] = baseIndex + radialSegments + 1;
        //         triangles[triangleIndex + 2] = baseIndex + 1;
        //         triangles[triangleIndex + 3] = baseIndex + 1;
        //         triangles[triangleIndex + 4] = baseIndex + radialSegments + 1;
        //         triangles[triangleIndex + 5] = baseIndex + radialSegments + 2;
        //     }

        //     if (interpolation && i == actualTrunkVertexCount - 2)
        //     {
        //         for (int j = 0; j < radialSegments; j++)
        //         {
        //             int baseIndex = (i+1) * (radialSegments + 1) + j;
        //             int triangleIndex = ((i+1) * radialSegments + j) * 6;

        //             triangles[triangleIndex] = baseIndex;
        //             triangles[triangleIndex + 1] = baseIndex + radialSegments + 1;
        //             triangles[triangleIndex + 2] = baseIndex + 1;
        //             triangles[triangleIndex + 3] = baseIndex + 1;
        //             triangles[triangleIndex + 4] = baseIndex + radialSegments + 1;
        //             triangles[triangleIndex + 5] = baseIndex + radialSegments + 2;
        //         }
        //     }
        // }

        if (actualTrunkVertexCount == 1 && interpolation)
        {
            triangles = new int[radialSegments * 6];
            for (int j = 0; j < radialSegments; j++)
            {
                int baseIndex = j;
                int triangleIndex = j * 6;

                triangles[triangleIndex] = baseIndex;
                triangles[triangleIndex + 1] = baseIndex + radialSegments + 1;
                triangles[triangleIndex + 2] = baseIndex + 1;
                triangles[triangleIndex + 3] = baseIndex + 1;
                triangles[triangleIndex + 4] = baseIndex + radialSegments + 1;
                triangles[triangleIndex + 5] = baseIndex + radialSegments + 2;
            }
        }
        else
        {
            triangles = new int[(actualTrunkVertexCount + (interpolation ? 0 : -1)) * radialSegments * 6];
            for (int i = 0; i < actualTrunkVertexCount - 1; i++)
            {
                for (int j = 0; j < radialSegments; j++)
                {
                    int baseIndex = i * (radialSegments + 1) + j;
                    int triangleIndex = (i * radialSegments + j) * 6;

                    triangles[triangleIndex] = baseIndex;
                    triangles[triangleIndex + 1] = baseIndex + radialSegments + 1;
                    triangles[triangleIndex + 2] = baseIndex + 1;
                    triangles[triangleIndex + 3] = baseIndex + 1;
                    triangles[triangleIndex + 4] = baseIndex + radialSegments + 1;
                    triangles[triangleIndex + 5] = baseIndex + radialSegments + 2;
                }

                if (interpolation && i == actualTrunkVertexCount - 2)
                {
                    for (int j = 0; j < radialSegments; j++)
                    {
                        int baseIndex = (i+1) * (radialSegments + 1) + j;
                        int triangleIndex = ((i+1) * radialSegments + j) * 6;

                        triangles[triangleIndex] = baseIndex;
                        triangles[triangleIndex + 1] = baseIndex + radialSegments + 1;
                        triangles[triangleIndex + 2] = baseIndex + 1;
                        triangles[triangleIndex + 3] = baseIndex + 1;
                        triangles[triangleIndex + 4] = baseIndex + radialSegments + 1;
                        triangles[triangleIndex + 5] = baseIndex + radialSegments + 2;
                    }
                }
            }
        }


        // Step 4: Assign the vertex array and triangle array to the Mesh
        trunkMesh.vertices = vertices;
        trunkMesh.triangles = triangles;
        trunkMesh.uv = uv;
        trunkMesh.RecalculateNormals();

        // Step 5: Return the Mesh
        return trunkMesh;
        //
    }

}
