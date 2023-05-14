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

    public static Mesh CreateBranchesMesh(List<Branch> branches, int radialSegments)
    {
        Mesh combinedMesh = new Mesh();
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        TreeVertex lastVertex = null;
        TreeVertex nextVertex = null;
        foreach (Branch branch in branches)
        {
            if (branch.SelfGrowthFactor == 0)
            {
                continue;
            }

            // 根据 SelfGrowthFactor 裁剪顶点列表
            int actualVertexCount = 0;
            for (int i = 0; i < branch.Vertices.Count; i++)
            {
                // Debug.Log("branch.LengthRatios " + i + " " + branch.LengthRatios[i] + " " + branch.SelfGrowthFactor);
                if (branch.LengthRatios[i] <= branch.SelfGrowthFactor)
                {
                    actualVertexCount++;
                }
                else
                {
                    break;
                }
            }

            // 插值系数
            float t = 0;

            // 从顶点列表中获取当前最新实际的顶点
            lastVertex = branch.Vertices[actualVertexCount - 1];

            // 从树枝顶点列表中获取下一个顶点，如果当前顶点是树枝顶点列表中的最后一个顶点，则下一个顶点为插值顶点
            if (branch.LengthRatios[actualVertexCount - 1] < 1)
            {
                nextVertex = branch.Vertices[actualVertexCount];
                t = (branch.SelfGrowthFactor - branch.LengthRatios[actualVertexCount - 1]) 
                / (branch.LengthRatios[actualVertexCount] - branch.LengthRatios[actualVertexCount - 1]);
            }
            else
            {
                nextVertex = lastVertex;
            }




            // 调整顶点半径
            for (int i = 0; i < actualVertexCount; i++){
                TreeVertex currentVertex = branch.Vertices[i];
                // 顶点半径
                float radiusScale = Mathf.Lerp(branch.MinRadiusFactor, branch.MaxRadiusFactor, branch.SelfGrowthFactor);
                currentVertex.RadiusScale = radiusScale;
            }

            // 更新插值顶点
            if (t > 0){
                Vector3 interpolatedPosition = Vector3.Lerp(lastVertex.Position, nextVertex.Position, t);
                float interpolatedRadiusX = Mathf.Lerp(lastVertex.RadiusX, nextVertex.RadiusX, t) * lastVertex.RadiusScale;
                branch.InterpolatedVertex = new TreeVertex(
                    nextVertex.Index,
                    interpolatedPosition,
                    nextVertex.Normal,
                    interpolatedRadiusX,
                    interpolatedRadiusX);
            }else{
                branch.InterpolatedVertex = null;
            }


            // 创建一个新的顶点列表，包含所需的顶点和插值顶点（如果有的话）
            List<TreeVertex> branchVerticesList = branch.Vertices.GetRange(0, actualVertexCount);

            // 添加插值顶点到顶点列表
            if (branch.InterpolatedVertex != null)
            {
                branchVerticesList.Add(branch.InterpolatedVertex);
            }

            Mesh branchMesh = MeshGenerator.CreateTrunkMesh(branchVerticesList, radialSegments, branchVerticesList.Count);
            CombineInstance combineInstance = new CombineInstance { mesh = branchMesh, transform = Matrix4x4.identity };
            combineInstances.Add(combineInstance);
        }

        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
        combinedMesh.RecalculateNormals();

        return combinedMesh;
    }


}
