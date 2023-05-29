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

    public CustomLeafShape customLeafShape;

    #region UI
    public Canvas canvas;
    public GameObject customLeafCreatorCanvas;
    public GameObject customLeafCreatorCanvas2;
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
        customLeafCreatorCanvas2 = GameObject.Find("CanvasLeafCreator2");
        customLeafCreatorCanvas.SetActive(false);
        customLeafCreatorCanvas2.SetActive(false);


        Transform leafObjectTransform = customLeafCreatorCanvas.transform.Find("LeafObject");
        if (leafObjectTransform != null) // Make sure the child was found
        {
            customLeafShape = leafObjectTransform.GetComponent<CustomLeafShape>();
        }
        
        // Text
        growthFactorText = panel.Find("GrowthFactorText").GetComponent<Text>();
        growthFactorText.text = $"Growth Factor: {growthFactor}";

        leavesNumberText = panel.Find("LeavesNumberText").GetComponent<Text>();
        leavesAvailableText = panel.Find("LeavesAvailableText").GetComponent<Text>();
        #endregion

        #region Camera

        cameraController = FindObjectOfType<CameraController>();
        #endregion


        // 1. Parse the tree skeleton file
        trunkVertices = TrunkParser.ParseTrunkVertices(trunkFilePath);
        branches = BranchParser.ParseBranches(trunkFilePath, 1);
        subBranches = BranchParser.ParseBranches(trunkFilePath, 2);

                
        // 1.1 Add an empty vertex to the trunkVertices list
        TreeVertex emptyVertex = new TreeVertex(-1, Vector3.zero, Vector3.zero, 0, 0);
        trunkVertices.Add(emptyVertex);

        // 1.2 Calculate the total length of the trunk
        trunkTotalLength = 0;
        for (int i = 0; i < trunkVertices.Count - 2; i++)
        {
            trunkTotalLength += Vector3.Distance(trunkVertices[i].Position, trunkVertices[i + 1].Position);
        }

        // 1.3 Set the length ratio of each trunk vertex

        trunkVerticesLengthRatios = new float[trunkVertices.Count];
        float currentLength = 0;
        for (int i = 0; i < trunkVertices.Count - 1; i++)
        {
            float lengthRatio = currentLength / trunkTotalLength;
            trunkVertices[i].LengthRatio = lengthRatio;
            trunkVerticesLengthRatios[i] = lengthRatio;
            currentLength += Vector3.Distance(trunkVertices[i].Position, trunkVertices[i + 1].Position);
        }


        // 1.4 Set the length ratio
        foreach (Branch branch in branches)
        {
            // Calculate the total length of the branch
            float branchTotalLength = 0;
            for (int i = 0; i < branch.Vertices.Count - 1; i++)
            {
                branchTotalLength += Vector3.Distance(branch.Vertices[i].Position, branch.Vertices[i + 1].Position);
            }

            // Set the length ratio of each branch vertex
            branch.LengthRatios = new float[branch.Vertices.Count];
            branch.StartGlobalGrowthFactors = new float[branch.Vertices.Count];

            // Set the first vertex of the branch as a fork
            branch.Vertices[0].IsFork = true;

            // Set the first vertex of the branch as the start vertex
            int startVertexIndex = branch.Vertices[0].Index;

            // Set the start vertex's global growth factor
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

            // Set the branch's min and max radius factor
            branch.MinRadiusFactor = branchMinRadiusFactor;
            branch.MaxRadiusFactor = branchMaxRadiusFactor;

        }

        // 1.5 Set the length ratio
        foreach (Branch branch in subBranches)
        {
            // Calculate the total length of the branch
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

            // Set the branch's min and max radius factor
            branch.MinRadiusFactor = branchMinRadiusFactor;
            branch.MaxRadiusFactor = branchMaxRadiusFactor;
        }


        // 1.6 Set the subBranches to the branches
        foreach (Branch branch in branches)
        {
            foreach (Branch subBranch in subBranches)
            {
                // If the subBranch's first vertex is the same as the branch's vertex, then add the subBranch to the branch

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
        
        // 2. Generate the trunk mesh and branches mesh
        Mesh trunkMesh = MeshGenerator.CreateTrunkMesh(trunkVertices, radialSegments, trunkVertices.Count - 1);
        Mesh branchesMesh = MeshGenerator.CreateBranchesMesh(branches, radialSegments);

        Mesh combinedMesh = CombineMeshes(trunkMesh, branchesMesh);


        // 3. Add MeshFilter to the trunk
        meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = combinedMesh;

        // 4. Add MeshRenderer to the trunk
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = barkMaterial;



        // 5. Generate the leaves
        leaves = LeafUtils.GetLeaves(branches);

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
            MeshGenerator.CreateLeavesMesh(transform, leaves, growthFactor, leavesNumber);

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

        // Calculate the actual vertex count based on the growth factor and the trunk vertices length ratios
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
    

        // Get the last vertex and the next vertex
        lastVertex = trunkVertices[actualVertexCount-1];

        // If the last vertex is the last vertex of the trunk, then the next vertex is the last vertex
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

        // Calculate the interpolated position and radius of the last vertex
        float t = (growthFactor - lastVertex.LengthRatio) / (nextVertex.LengthRatio - lastVertex.LengthRatio);


        Vector3 interpolatedPosition = Vector3.Lerp(lastVertex.Position, nextVertex.Position, t);


        float interpolatedRadiusX = Mathf.Lerp(lastVertex.RadiusX, nextVertex.RadiusX, t) * lastVertex.RadiusScale;

        // Update the last vertex 
        trunkVertices[trunkVertices.Count - 1] = new TreeVertex(
            nextVertex.Index,
            interpolatedPosition,
            nextVertex.Normal,
            interpolatedRadiusX,
            interpolatedRadiusX
        );
    }


    private void UpdateBranches()
    {
        for (int i = 0; i < branches.Count; i++)
        {
            Branch branch = branches[i];

            // Set the min radius and max radius of the branch
            branch.MinRadiusFactor = branchMinRadiusFactor;
            branch.MaxRadiusFactor = branchMaxRadiusFactor;

            // Find the start vertex index of the branch
            float startFromStartGlobalGrowthFactor = branch.StartGlobalGrowthFactors[0];

            if ( growthFactor >= startFromStartGlobalGrowthFactor)
            {
                
                // Calculate the self growth factor of the branch
                branch.SelfGrowthFactor = (growthFactor - startFromStartGlobalGrowthFactor) / (1 - startFromStartGlobalGrowthFactor);

            }else{
                branch.SelfGrowthFactor = 0;
                continue;
            }


            foreach (Branch subBranch in branch.SubBranches)
            {

                // Set the min radius and max radius of the sub branch
                subBranch.MinRadiusFactor = branchMinRadiusFactor;
                subBranch.MaxRadiusFactor = branchMaxRadiusFactor;

                // Find the start vertex index of the sub branch
                int subBranchStartVertexIndex = subBranch.Vertices[0].Index;
                float subBranchStartVertexLengthRatio = -1.0f;


                // Find the length ratio of the start vertex of the sub branch
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
                    // Calculate the self growth factor of the sub branch
                    subBranch.SelfGrowthFactor = (branch.SelfGrowthFactor - subBranchStartVertexLengthRatio) / (1 - subBranchStartVertexLengthRatio);
                }
                else
                {
                    subBranch.SelfGrowthFactor = 0;
                }
            }
        }

    }


    // Combine two meshes
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
        // Disable CustomLeafCreatorCanvas
        canvas.enabled = false;
        gameObject.SetActive(false);


        // Enable CustomLeafCreatorCanvas
        customLeafCreatorCanvas.SetActive(true);
        customLeafCreatorCanvas2.SetActive(true);
        customLeafShape.ResetToDefault();

    }

    public void AddLeafEventContinue(LeafData data)
    {
        canvas.enabled = true;
        gameObject.SetActive(true);
        customLeafCreatorCanvas.SetActive(false);
        customLeafCreatorCanvas2.SetActive(false);


        if (leavesNumber == leaves.Count)
        {
            return;
        }

        int availableLeavesNumber = LeafUtils.GetAvailableLeavesNumber(leaves, growthFactor);

        leaves[leavesNumber].LeafData = data;

        nextCameraPosition = LeafUtils.GetNextAvailableLeafPosition(leaves, growthFactor);
        autoGrow = true;


        // if (availableLeavesNumber > leavesNumber)
        // {
        //     leavesNumber++;
        //     treeUpdated = true;
        // }else{
        //     nextCameraPosition = LeafUtils.GetNextAvailableLeafPosition(leaves, growthFactor);
        //     autoGrow = true;
        // }
    }
}
