using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFromSkeleton : MonoBehaviour
{
    [Range(0, 1)]
    public float growthFactor = 0.0f;

    [SerializeField]
    private int radialSegments = 8;


    public float trunkMinRadiusFactor = 0.75f;

    public float trunkMaxRadiusFactor = 1.25f;

    private List<TreeVertex> trunkVertices;

    private List<Branch> branches;

    private float trunkTotalLength;

    private float[] trunkVerticesLengthRatios;

    private string trunkFilePath = "Assets/Scripts/TreeMesh/Tree-mesh.txt";

    private MeshFilter meshFilter;

    private MeshRenderer meshRenderer;

    private int actualVertexCount;

    private bool treeUpdated = false;

    // Start is called before the first frame update
    void Start()
    {
        // 1. 解析树干和树枝的顶点
        trunkVertices = TrunkParser.ParseTrunkVertices(trunkFilePath);
        branches = BranchParser.ParseBranches(trunkFilePath, 1);
        
        // 1.1 在树干顶点组最后添加一个顶点，作为预留的插值顶点
        TreeVertex emptyVertex = new TreeVertex(-1, Vector3.zero, Vector3.zero, 0, 0);
        trunkVertices.Add(emptyVertex);

        // 1.2 计算树干的总长度
        trunkTotalLength = 0;
        for (int i = 0; i < trunkVertices.Count - 2; i++)
        {
            trunkTotalLength += Vector3.Distance(trunkVertices[i].Position, trunkVertices[i + 1].Position);
        }

        // 1.3 设置每一个顶点的长度比例

        trunkVerticesLengthRatios = new float[trunkVertices.Count];
        float currentLength = 0;
        for (int i = 0; i < trunkVertices.Count - 1; i++)
        {
            float lengthRatio = currentLength / trunkTotalLength;
            trunkVertices[i].LengthRatio = lengthRatio;
            trunkVerticesLengthRatios[i] = lengthRatio;
            currentLength += Vector3.Distance(trunkVertices[i].Position, trunkVertices[i + 1].Position);
        }



        // 1.4 设置树枝的长度比例
        foreach (Branch branch in branches)
        {
            // 计算树枝的总长度
            float branchTotalLength = 0;
            for (int i = 0; i < branch.Vertices.Count - 1; i++)
            {
                branchTotalLength += Vector3.Distance(branch.Vertices[i].Position, branch.Vertices[i + 1].Position);
            }

            branch.LengthRatios = new float[branch.Vertices.Count];
            currentLength = 0;
            for (int i = 0; i < branch.Vertices.Count; i++)
            {
                float lengthRatio = currentLength / branchTotalLength;
                branch.LengthRatios[i] = lengthRatio;

                if (i == branch.Vertices.Count - 1)
                {
                    branch.LengthRatios[i] = 1.0f;
                }else{
                    currentLength += Vector3.Distance(branch.Vertices[i].Position, branch.Vertices[i + 1].Position);
                }
            }


            // Print the length ratios of the branch
            foreach (float lengthRatio in branch.LengthRatios)
            {
                Debug.Log(lengthRatio);
            }

            Debug.Log("Branch total length: " + branchTotalLength);



        }


        
        // 2. 使用解析出的顶点创建树干网格
        Mesh trunkMesh = MeshGenerator.CreateTrunkMesh(trunkVertices, radialSegments, trunkVertices.Count - 1);
        Mesh branchesMesh = MeshGenerator.CreateBranchesMesh(branches, radialSegments);

        Mesh combinedMesh = CombineMeshes(trunkMesh, branchesMesh);


        // 3. 将生成的树干网格赋给 MeshFilter 组件
        meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = combinedMesh;

        // 4. 为树干添加 MeshRenderer
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }




    }

    
    void Update()
    {
        if (treeUpdated)
        {
            UpdateTrunk();
            UpdateBranches();
            Mesh updatedTrunkMesh = MeshGenerator.CreateTrunkMesh(trunkVertices, radialSegments, actualVertexCount);
            Mesh branchesMesh = MeshGenerator.CreateBranchesMesh(branches, radialSegments);


            Mesh combinedMesh = CombineMeshes(updatedTrunkMesh, branchesMesh);
            meshFilter.mesh = combinedMesh;
            treeUpdated = false;
        }
    }

    void OnValidate()
    {
        treeUpdated = true;
    }

    private void UpdateTrunk()
    {
        TreeVertex lastVertex = null;
        TreeVertex nextVertex = null;

        if (trunkVertices.Count < 2)
        {
            return;
        }

        // 根据 growthFactor 和 trunkVerticesLengthRatios 计算实际的顶点数量
        actualVertexCount = 0;
        for (int i = 0; i < trunkVerticesLengthRatios.Length - 1; i++)
        {
            if (trunkVerticesLengthRatios[i] <= growthFactor)
            {
                actualVertexCount++;
            }
            else
            {
                break;
            }
        }
    

        // 从树干顶点组中获取当前最新实际的顶点
        lastVertex = trunkVertices[actualVertexCount-1];

        // 从树干顶点组中获取下一个顶点, 如果当前顶点的长度比例为 1，则下一个顶点为当前顶点
        if (lastVertex.LengthRatio < 1)
        {
            nextVertex = trunkVertices[actualVertexCount];
        }
        else
        {
            nextVertex = lastVertex;
        }

        for (int i = 0; i < actualVertexCount; i++)
        {
            TreeVertex vertex = trunkVertices[i];
            float radiusFactor = Mathf.Lerp(trunkMinRadiusFactor, trunkMaxRadiusFactor, growthFactor);
            vertex.RadiusScale = radiusFactor ;
        }

        // 计算插值后的位置
        float t = (growthFactor - lastVertex.LengthRatio) / (nextVertex.LengthRatio - lastVertex.LengthRatio);


        Vector3 interpolatedPosition = Vector3.Lerp(lastVertex.Position, nextVertex.Position, t);


        float interpolatedRadiusX = Mathf.Lerp(lastVertex.RadiusX, nextVertex.RadiusX, t) * lastVertex.RadiusScale;

        // 更新插值顶点(最后一个顶点)
        trunkVertices[trunkVertices.Count - 1] = new TreeVertex(
            nextVertex.Index,
            interpolatedPosition,
            nextVertex.Normal,
            interpolatedRadiusX,
            interpolatedRadiusX
        );


        // Debug.Log("lastVertex: " + lastVertex.Position + ", nextVertex: " + nextVertex.Position + ", interpolatedPosition: " + interpolatedPosition);
    }


    private void UpdateBranches()
    {
        for (int i = 0; i < branches.Count; i++)
        {
            Branch branch = branches[i];

            // 寻找起始顶点的index
            int startVertexIndex = branch.Vertices[0].Index;

            // 在树干顶点组中找到起始顶点
            float startVertexLengthRatio = trunkVertices.Find(vertex => vertex.Index == startVertexIndex).LengthRatio;


            if (startVertexLengthRatio >= growthFactor)
            {
                // 计算树枝的 SelfGrowthFactor
                branch.SelfGrowthFactor = (growthFactor - startVertexLengthRatio) / (1 - startVertexLengthRatio);
            }else{
                branch.SelfGrowthFactor = 0;
            }

            // 更新分支的顶点半径和插值顶点
            for (int j = 0; j < branch.Vertices.Count - 1; j++)
            {
                TreeVertex currentVertex = branch.Vertices[j];
                TreeVertex nextVertex = branch.Vertices[j + 1];

                float radiusScale = Mathf.Lerp(trunkMinRadiusFactor, trunkMaxRadiusFactor, branch.SelfGrowthFactor);
                currentVertex.RadiusScale = radiusScale;

                if (j == branch.Vertices.Count - 2) // 最后一个顶点之前
                {
                    float t = branch.SelfGrowthFactor;
                    Vector3 interpolatedPosition = Vector3.Lerp(currentVertex.Position, nextVertex.Position, t);
                    float interpolatedRadiusX = Mathf.Lerp(currentVertex.RadiusX, nextVertex.RadiusX, t) * currentVertex.RadiusScale;

                    // 更新插值顶点
                    branch.InterpolatedVertex = new TreeVertex(
                        nextVertex.Index,
                        interpolatedPosition,
                        nextVertex.Normal,
                        interpolatedRadiusX,
                        interpolatedRadiusX
                    );
                }
            }
        }
    }



    // 合并两个 Mesh 对象的方法
    private Mesh CombineMeshes(Mesh mesh1, Mesh mesh2)
    {
        Mesh combinedMesh = new Mesh();

        // Combine meshes
        CombineInstance[] combineInstances = new CombineInstance[2];
        combineInstances[0].mesh = mesh1;
        combineInstances[0].transform = Matrix4x4.identity;
        combineInstances[1].mesh = mesh2;
        combineInstances[1].transform = Matrix4x4.identity;

        combinedMesh.CombineMeshes(combineInstances);
        combinedMesh.RecalculateNormals();

        return combinedMesh;
    }



}
