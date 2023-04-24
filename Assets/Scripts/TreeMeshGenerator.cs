using UnityEngine;
using System.Collections.Generic;

public class TreeMeshGenerator
{
    public static Mesh CreateTrunkMesh(Vector3[] trunkVertices, float trunkRadius, int radialSegments)
    {
        // 方法实现

        // Step 1: Create a new Mesh instance
        Mesh trunkMesh = new Mesh();

        // Step 2: Create a ring for each vertex and store them in a vertex array
        int vertexCount = trunkVertices.Length * (radialSegments + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];

        for (int i = 0; i < trunkVertices.Length; i++)
        {
            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = 2 * Mathf.PI * j / radialSegments;
                float x = trunkRadius * Mathf.Cos(angle);
                float z = trunkRadius * Mathf.Sin(angle);

                Vector3 vertexOffset = new Vector3(x, 0, z);
                vertices[i * (radialSegments + 1) + j] = trunkVertices[i] + vertexOffset;
                uv[i * (radialSegments + 1) + j] = new Vector2((float)j / radialSegments, (float)i / (trunkVertices.Length - 1));
            }
        }

        // Step 3: Create triangles based on the vertex array and store them in a triangle array
        int[] triangles = new int[(trunkVertices.Length - 1) * radialSegments * 6];
        for (int i = 0; i < trunkVertices.Length - 1; i++)
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


    public static Mesh CreateBranchMesh(TreeBranch branch, float branchRadius, int radialSegments)
    {
        Vector3[] branchVertices = branch.vertices;

        // Step 1: Create a new Mesh instance
        Mesh branchMesh = new Mesh();

        // Step 2: Create a ring for each vertex and store them in a vertex array
        int vertexCount = branchVertices.Length * (radialSegments + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];

        for (int i = 0; i < branchVertices.Length; i++)
        {
            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = 2 * Mathf.PI * j / radialSegments;
                float x = branchRadius * Mathf.Cos(angle);
                float z = branchRadius * Mathf.Sin(angle);

                Vector3 vertexOffset = new Vector3(x, 0, z);
                vertices[i * (radialSegments + 1) + j] = branchVertices[i] + vertexOffset;
                uv[i * (radialSegments + 1) + j] = new Vector2((float)j / radialSegments, (float)i / (branchVertices.Length - 1));
            }
        }

        // Step 3: Create triangles based on the vertex array and store them in a triangle array
        int[] triangles = new int[(branchVertices.Length - 1) * radialSegments * 6];
        for (int i = 0; i < branchVertices.Length - 1; i++)
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
        }

        // Step 4: Assign the vertex array and triangle array to the Mesh
        branchMesh.vertices = vertices;
        branchMesh.triangles = triangles;
        branchMesh.uv = uv;
        branchMesh.RecalculateNormals();

        // Step 5: Return the Mesh
        return branchMesh;
    }


}
