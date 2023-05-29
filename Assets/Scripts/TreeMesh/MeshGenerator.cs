using UnityEngine;
using System.Collections.Generic;
using static LeafDataUtils;
using UnityEditor;


public class MeshGenerator
{
    public static Mesh CreateTrunkMesh(List<TreeVertex> trunkVerticesList, int radialSegments, int actualTrunkVertexCount)
    {

        Vector3[] trunkVertices = trunkVerticesList.ConvertAll(vertex => vertex.Position).ToArray();

        bool interpolation = actualTrunkVertexCount < trunkVerticesList.Count - 1;
        int interpolationVertexIndex = trunkVerticesList.Count - 1;

        Mesh trunkMesh = new Mesh();

        int vertexCount = (actualTrunkVertexCount + (interpolation ? 1 : 0)) * (radialSegments + 1);

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];

        Vector3 upDirection = Vector3.up;

        for (int i = 0; i < actualTrunkVertexCount; i++)
        {
            float trunkRadius = trunkVerticesList[i].RadiusX * trunkVerticesList[i].RadiusScale;

            if (i < trunkVertices.Length - 1)
                {
                    upDirection = (trunkVertices[i + 1] - trunkVertices[i]).normalized;
                }

            Vector3 rightDirection = Vector3.Cross(upDirection, Vector3.forward).normalized;
            Vector3 forwardDirection = Vector3.Cross(rightDirection, upDirection).normalized;

            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = 2 * Mathf.PI * j / radialSegments;
                float x = trunkRadius * Mathf.Cos(angle);
                float z = trunkRadius * Mathf.Sin(angle);

                Vector3 vertexOffset = rightDirection * x + forwardDirection * z;
                vertices[i * (radialSegments + 1) + j] = trunkVertices[i] + vertexOffset;
                uv[i * (radialSegments + 1) + j] = new Vector2((float)j / radialSegments, (float)i / (actualTrunkVertexCount - 1));
            }

            if (interpolation && i == actualTrunkVertexCount - 1)
            {
                trunkRadius = trunkVerticesList[interpolationVertexIndex].RadiusX * trunkVerticesList[interpolationVertexIndex].RadiusScale;
                for (int j = 0; j <= radialSegments; j++)
                {
                    float angle = 2 * Mathf.PI * j / radialSegments;
                    float x = trunkRadius * Mathf.Cos(angle);
                    float z = trunkRadius * Mathf.Sin(angle);

                    Vector3 vertexOffset = rightDirection * x + forwardDirection * z;
                    vertices[(i+1) * (radialSegments + 1) + j] = trunkVertices[interpolationVertexIndex] + vertexOffset;
                    uv[(i+1) * (radialSegments + 1) + j] = new Vector2((float)j / radialSegments, (float)(i+1) / (actualTrunkVertexCount - 1));
                    // TODO: fix UVs, 当actualTrunkVertexCount == 1时，uv[(i+1) * (radialSegments + 1) + j] = 0
                }
            }

        }

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

        trunkMesh.vertices = vertices;
        trunkMesh.triangles = triangles;
        trunkMesh.uv = uv;
        trunkMesh.RecalculateNormals();

        return trunkMesh;
    }



    public static Mesh CreateBranchesMesh(List<Branch> branches, int radialSegments, bool sub = false)
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

            // Get actual vertex count
            int actualVertexCount = 0;
            for (int i = 0; i < branch.Vertices.Count; i++)
            {
                if (branch.LengthRatios[i] <= branch.SelfGrowthFactor)
                {
                    actualVertexCount++;
                }
                else
                {
                    break;
                }
            }

            // Interpolation factor
            float t = 0;

            // Get last vertex and next vertex
            lastVertex = branch.Vertices[actualVertexCount - 1];

            // If the last vertex is not the last vertex of the branch
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


            // Update radius scale
            for (int i = 0; i < actualVertexCount; i++){
                TreeVertex currentVertex = branch.Vertices[i];
                
                float radiusScale = Mathf.Lerp(branch.MinRadiusFactor, branch.MaxRadiusFactor, branch.SelfGrowthFactor);
                currentVertex.RadiusScale = radiusScale;
            }

            // Interpolate vertex
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


            // Create a new list of vertices, which contains the actual vertices and the interpolated vertex
            List<TreeVertex> branchVerticesList = branch.Vertices.GetRange(0, actualVertexCount);

            // Add the interpolated vertex to the list if it exists
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


    public static Mesh CreateLeavesMesh(Transform parentTransform, List<Leaf> leaves, float growthFactor, int leafNumber)
    {

        foreach (Transform child in parentTransform)
        {
            if (child.name == "Leaf")
            {
                // Destroy(child.gameObject);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }


        for (int i = 0; i < Mathf.Min(leafNumber, leaves.Count); i++)
        {
            Leaf leaf = leaves[i];

            GameObject leafObject = new GameObject("Leaf");
            leafObject.transform.SetParent(parentTransform);

            if (leaf.StartGlobalGrowthFactor <= growthFactor)
            {

                Mesh leafMesh = CreateMeshFromLeafData(leaf.LeafData);

                Matrix4x4 transformMatrix = Matrix4x4.TRS(leaf.Position, Quaternion.identity, Vector3.one);

                MeshFilter meshFilter = leafObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = leafObject.AddComponent<MeshRenderer>();

                // Set mesh to leafMesh
                meshFilter.mesh = leafMesh;

                // Set material to leaf material

                string materialPath = "Assets/Material/Leaf.mat";

                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                meshRenderer.material = material;



                leafObject.transform.position = leaf.Position;


                leafObject.transform.rotation = Quaternion.LookRotation(leaf.GrowthDirection, leaf.UpSide ? Vector3.up : Vector3.down);
                leafObject.transform.Rotate(0, 90, 0, Space.Self);

                Vector3 offsetDirection = leafObject.transform.up;
                float offsetAmount = leaf.BranchRadius * growthFactor;

                if (!leaf.UpSide)
                {
                    offsetDirection *= -1;  // If the leaf is upside down, the offset direction should be reversed
                }

                leafObject.transform.Translate(offsetDirection * offsetAmount, Space.Self);

                leafObject.transform.localScale = Vector3.one * leaf.Scale;

                Color leafColor = Color.HSVToRGB(leaf.LeafData.leafHue, leaf.LeafData.leafSaturation, leaf.LeafData.leafBrightness);

                MaterialPropertyBlock mpb = new MaterialPropertyBlock();

                mpb.SetColor("_Color", leafColor);

                meshRenderer.SetPropertyBlock(mpb);






            }

        }
        return null;
    }

}
