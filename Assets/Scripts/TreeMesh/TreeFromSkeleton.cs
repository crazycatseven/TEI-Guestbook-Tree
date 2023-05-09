using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeFromSkeleton : MonoBehaviour
{
    [Range(0, 1)]
    public float growthFactor = 1.0f;

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

    // Update is called once per frame
    void Update()
    {
        Grow();
        Mesh updatedTrunkMesh = MeshGenerator.CreateTrunkMesh(trunkVertices, radialSegments, actualVertexCount);
        Mesh branchesMesh = MeshGenerator.CreateBranchesMesh(branches, radialSegments);


        Mesh combinedMesh = CombineMeshes(updatedTrunkMesh, branchesMesh);
        meshFilter.mesh = combinedMesh;



    }

    private void Grow()
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


    private void parseB()
    {
        string path = "Assets/Scripts/TreeMesh/Tree-mesh.txt";
        List<Branch> branches = BranchParser.ParseBranches(path, 1);


        Debug.Log("branches.Count: " + branches.Count);

        foreach (Branch branch in branches)
        {
            Debug.Log("branch: " + branch);
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
