using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static LeafDataUtils;

public class TreeFromSkeleton : MonoBehaviour
{

    public Material barkMaterial;

    [Range(0, 1)]
    public float growthFactor = 0.0f;

    [Range(0, 100)]
    public int leavesNumber = 0;

    [SerializeField]
    private int radialSegments = 16;

    public float trunkMinRadiusFactor = 0.75f;

    public float trunkMaxRadiusFactor = 1.25f;

    public float branchMinRadiusFactor = 0.25f;

    public float branchMaxRadiusFactor = 1.25f;

    private List<TreeVertex> trunkVertices;

    private List<Branch> branches;

    private List<Leaf> leaves;

    private List<Branch> subBranches;

    private float trunkTotalLength;

    private float[] trunkVerticesLengthRatios;

    private string trunkFilePath = "Assets/Scripts/TreeMesh/Tree-mesh.txt";

    private MeshFilter meshFilter;

    private MeshRenderer meshRenderer;

    private int actualVertexCount;

    public bool treeUpdated = false;

    private bool autoGrow = false;

    #region UI
    public Canvas canvas;
    public GameObject customLeafCreatorCanvas;
    private Text growthFactorText;
    private Text leavesNumberText;
    private Text leavesAvailableText;
    #endregion

    #region Camera
    public CameraController cameraController;
    private Vector3 nextCameraPosition;
    #endregion
    

    // Start is called before the first frame update
    void Start()
    {
        #region UI Event
        Transform panel = canvas.transform.Find("Panel");
        customLeafCreatorCanvas = GameObject.Find("CustomLeafCreator");
        customLeafCreatorCanvas.SetActive(false);
        
        // Text
        growthFactorText = panel.Find("GrowthFactorText").GetComponent<Text>();
        growthFactorText.text = $"Growth Factor: {growthFactor}";

        leavesNumberText = panel.Find("LeavesNumberText").GetComponent<Text>();
        leavesAvailableText = panel.Find("LeavesAvailableText").GetComponent<Text>();
        #endregion

        #region Camera

        cameraController = FindObjectOfType<CameraController>();
        #endregion


        // 1. 解析树干和树枝的顶点
        trunkVertices = TrunkParser.ParseTrunkVertices(trunkFilePath);
        branches = BranchParser.ParseBranches(trunkFilePath, 1);
        subBranches = BranchParser.ParseBranches(trunkFilePath, 2);

                
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

            // 设置树枝的长度比例与起始生长因子组
            branch.LengthRatios = new float[branch.Vertices.Count];
            branch.StartGlobalGrowthFactors = new float[branch.Vertices.Count];

            // 把起始点设置为分叉点
            branch.Vertices[0].IsFork = true;

            // 寻找起始顶点的index
            int startVertexIndex = branch.Vertices[0].Index;

            // 在树干顶点组中找到起始顶点
            float startVertexLengthRatio = trunkVertices.Find(vertex => vertex.Index == startVertexIndex).LengthRatio;
            
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

                branch.StartGlobalGrowthFactors[i] = startVertexLengthRatio + branch.LengthRatios[i] * (1 - startVertexLengthRatio);
            }

            // 设置树枝的最小半径和最大半径
            branch.MinRadiusFactor = branchMinRadiusFactor;
            branch.MaxRadiusFactor = branchMaxRadiusFactor;

        }

        // 1.5 设置子树枝的长度比例
        foreach (Branch branch in subBranches)
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
                }
                else
                {
                    currentLength += Vector3.Distance(branch.Vertices[i].Position, branch.Vertices[i + 1].Position);
                }
            }

            // 设置树枝的最小半径和最大半径
            branch.MinRadiusFactor = branchMinRadiusFactor;
            branch.MaxRadiusFactor = branchMaxRadiusFactor;
        }


        // 1.6 把子树枝添加到树枝的子树枝列表中
        foreach (Branch branch in branches)
        {
            foreach (Branch subBranch in subBranches)
            {
                // 如果子树枝的起始顶点在树枝的定点组中，则把子树枝添加到树枝的子树枝列表中

                for (int i = 0; i < branch.Vertices.Count; i++)
                {
                    if (subBranch.Vertices[0].Index == branch.Vertices[i].Index)
                    {
                        subBranch.Vertices[0].IsFork = true;
                        branch.Vertices[i].IsFork = true;


                        branch.SubBranches.Add(subBranch);

                        subBranch.StartGlobalGrowthFactors = new float[subBranch.Vertices.Count];
                        for (int j = 0; j < subBranch.Vertices.Count; j++)
                        {
                            subBranch.StartGlobalGrowthFactors[j] = branch.StartGlobalGrowthFactors[i] 
                                + subBranch.LengthRatios[j] * (1 - branch.StartGlobalGrowthFactors[i]);
                        }
                        break;
                    }
                }
            }
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
        meshRenderer.material = barkMaterial;



        // 5. 获得所有树叶
        leaves = LeafUtils.GetLeaves(branches);


        // MeshGenerator.CreateLeavesMesh(transform, leaves, growthFactor * 0.1f, leavesNumber);

        // // 创建叶子的网格
        // // Mesh leavesMesh = MeshGenerator.CreateLeavesMesh(leaves, growthFactor * 0.1f, leavesNumber, leafPrefab);

        // // 创建一个新的子游戏对象
        // GameObject leavesObject = new GameObject("LeafObject");

        // // 把新的子游戏对象设置为当前游戏对象的子对象
        // leavesObject.transform.parent = this.transform;

        // // 把叶子的网格赋给新的子游戏对象的 MeshFilter 组件
        // MeshFilter leavesMeshFilter = leavesObject.AddComponent<MeshFilter>();
        // leavesMeshFilter.mesh = leavesMesh;

        // // 为叶子添加 MeshRenderer
        // MeshRenderer leavesMeshRenderer = leavesObject.AddComponent<MeshRenderer>();


        treeUpdated = true;

    }

    
    void Update()
    {
        if (treeUpdated || autoGrow)
        {
            if (autoGrow)
            {
                int availableLeavesNumber = LeafUtils.GetAvailableLeavesNumber(leaves, growthFactor);
                if (availableLeavesNumber <= leavesNumber)
                {
                    growthFactor += 0.0001f;
                }
                else{
                    autoGrow = false;
                    StartCoroutine(cameraController.MoveCameraAndReturn(nextCameraPosition, new Vector3(0, 3, -6), 2.0f));
                }
                
            }

            UpdateTrunk();
            UpdateBranches();
            UpdateUI();
            Mesh updatedTrunkMesh = MeshGenerator.CreateTrunkMesh(trunkVertices, radialSegments, actualVertexCount);
            Mesh branchesMesh = MeshGenerator.CreateBranchesMesh(branches, radialSegments);
            
            foreach (Branch branch in branches)
            {
                branchesMesh = CombineMeshes(branchesMesh, MeshGenerator.CreateBranchesMesh(branch.SubBranches, radialSegments, true));
            } 

            Mesh combinedMesh = CombineMeshes(updatedTrunkMesh, branchesMesh);

            meshFilter.mesh = combinedMesh;

            // Create leaves mesh
            // MeshGenerator.CreateLeavesMesh(leaves, growthFactor, leavesNumber, leafPrefab);
            Debug.Log("before Create");
            MeshGenerator.CreateLeavesMesh(transform, leaves, growthFactor, leavesNumber);

            // Find the leaf object
            // Transform leafTransform = transform.Find("LeafObject");
            // if (leafTransform == null)
            // {
            //     // The leaf object does not exist, create it
            //     GameObject leafObject = new GameObject("LeafObject");
            //     leafObject.transform.parent = transform;
            //     leafObject.AddComponent<MeshFilter>();
            //     leafObject.AddComponent<MeshRenderer>();
            //     leafTransform = leafObject.transform;
            // }

            // Update the mesh of the leaf object
            // MeshFilter leafMeshFilter = leafTransform.GetComponent<MeshFilter>();
            // leafMeshFilter.mesh = leavesMesh;

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
            vertex.RadiusScale = radiusFactor;
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

            // 设置树枝的最小半径和最大半径
            branch.MinRadiusFactor = branchMinRadiusFactor;
            branch.MaxRadiusFactor = branchMaxRadiusFactor;

            // 在树干顶点组中找到起始顶点
            float startFromStartGlobalGrowthFactor = branch.StartGlobalGrowthFactors[0];

            if ( growthFactor >= startFromStartGlobalGrowthFactor)
            {
                
                // 计算树枝的 SelfGrowthFactor
                branch.SelfGrowthFactor = (growthFactor - startFromStartGlobalGrowthFactor) / (1 - startFromStartGlobalGrowthFactor);

            }else{
                branch.SelfGrowthFactor = 0;
                continue;
            }


            foreach (Branch subBranch in branch.SubBranches)
            {

                           // 设置树枝的最小半径和最大半径
                subBranch.MinRadiusFactor = branchMinRadiusFactor;
                subBranch.MaxRadiusFactor = branchMaxRadiusFactor;

                // 寻找子树枝的起始顶点的index
                int subBranchStartVertexIndex = subBranch.Vertices[0].Index;
                float subBranchStartVertexLengthRatio = -1.0f;


                // 在树枝顶点组中找到子树枝的起始顶点
                for (int j = 0; j < branch.Vertices.Count; j++)
                {
                    if (branch.Vertices[j].Index == subBranchStartVertexIndex)
                    {
                        subBranchStartVertexLengthRatio = branch.LengthRatios[j];
                        break;
                    }
                }

                if (branch.SelfGrowthFactor >= subBranchStartVertexLengthRatio)
                {
                    // 计算subBranch的 SelfGrowthFactor
                    subBranch.SelfGrowthFactor = (branch.SelfGrowthFactor - subBranchStartVertexLengthRatio) / (1 - subBranchStartVertexLengthRatio);
                }
                else
                {
                    subBranch.SelfGrowthFactor = 0;
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


    private void UpdateUI()
    {
        growthFactorText.text = $"Growth Factor: {growthFactor}";
        leavesNumberText.text = $"Leaves Number: {leavesNumber}";
        leavesAvailableText.text = $"Leaves Available: {LeafUtils.GetAvailableLeavesNumber(leaves, growthFactor)}";
    }


    public void AddLeafEvent()
    {
        // 隐藏UI和当前的GameObject
        canvas.enabled = false;
        gameObject.SetActive(false);


        // 启用CustomLeafCreatorCanvas
        customLeafCreatorCanvas.SetActive(true);
    }

    public void AddLeafEventContinue(LeafData data)
    {
        canvas.enabled = true;
        gameObject.SetActive(true);
        customLeafCreatorCanvas.SetActive(false);


        if (leavesNumber == leaves.Count)
        {
            return;
        }

        int availableLeavesNumber = LeafUtils.GetAvailableLeavesNumber(leaves, growthFactor);

        leaves[leavesNumber].LeafData = data;

        if (availableLeavesNumber > leavesNumber)
        {
            leavesNumber++;
            treeUpdated = true;
        }else{
            nextCameraPosition = LeafUtils.GetNextAvailableLeafPosition(leaves, growthFactor);
            autoGrow = true;
        }
    }
}
