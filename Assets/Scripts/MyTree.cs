using UnityEngine;
using System.Collections.Generic;
using System.Linq;



public class MyTree : MonoBehaviour
{

    public Material trunkMaterial;


    public float treeHeight = 10f;
    public int numSegments = 10;


    [Range(0, 100)]
    public int totalLeafCount = 50;


    [Range(0,1 )]
    public float twistIntensity = 0.3f;
    public AnimationCurve twistCurve;

    [Range(0, 10)]
    public int mainBranchesNumber = 4;

    [Range(0, 1)]
    public float growingFactor = 1.0f;

    [Range(0, 180)]
    public float minAngleBetweenBranches = 20f;

    [Range(0, 180)]
    public float anglePerturbation = 10f;

    [Range(0, 1)]
    public float branchStartFactor = 0.5f;

    private Vector3[] vertices;
    public List<TreeBranch> mainBranches;

    public int trunkTwistSeed = 42;
    private float trunkTwistRandomNumber;

    private List<Vector3> allLeafPositions;

    void Start()
    {
        // Get the mesh filter and renderer
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();



        // Initialize the trunk twist curve
        twistCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        Random.InitState(trunkTwistSeed);
        trunkTwistRandomNumber = Random.Range(-0.5f, 0.5f);

        // Generate the trunk vertices
        GenerateVertices();


        // Initialize the main branches list
        mainBranches = new List<TreeBranch>();

        // Generate the main branches
        GenerateMainBranches();

    }

    private void GenerateVertices()
    {

        Random.InitState(trunkTwistSeed);
        trunkTwistRandomNumber = Random.Range(-0.5f, 0.5f);

        int actualNumSegments = Mathf.FloorToInt(numSegments * growingFactor);
        vertices = new Vector3[actualNumSegments + 1];
        float segmentHeight = treeHeight / numSegments;

        Vector3 lastOffset = Vector3.zero;
        for (int i = 0; i <= actualNumSegments; i++)
        {
            float y = i * segmentHeight + trunkTwistRandomNumber;

            // float y = i * segmentHeight;
            float t = (float)i / numSegments;
            float growingTwistIntensity = twistIntensity * growingFactor;
            float twistAmount = twistCurve.Evaluate(t) * growingTwistIntensity;

            

            Vector3 randomDirection = new Vector3(
                Mathf.PerlinNoise(y, 0) - 0.5f,
                0,
                Mathf.PerlinNoise(0, y) - 0.5f
            ).normalized;

            lastOffset += randomDirection * segmentHeight * twistAmount;
            vertices[i] = new Vector3(0, y, 0) + lastOffset;
        }
    }

    private void GenerateMainBranches()
    {

        mainBranches.Clear();

        int branchStartFromSegment = Mathf.FloorToInt(numSegments * branchStartFactor);

        int actualNumSegments = Mathf.FloorToInt(numSegments * growingFactor);

        int actualBranchesNumber = Mathf.Min(mainBranchesNumber, actualNumSegments - branchStartFromSegment);

        float anglePerBranch = 360f / actualBranchesNumber;

        for (int i = 0; i < actualBranchesNumber; i++)
        {
            int branchSeed = 100 + i;
            
            // Randomly choose the branch height
            float branchHeight = Random.Range(3.0f, 7.0f);

            AnimationCurve branchTwistCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            float actualBranchGrowingFactor = Mathf.Clamp01((growingFactor - branchStartFactor) / (1 - branchStartFactor));

            mainBranches.Add(new TreeBranch(5, 0.3f, branchTwistCurve, branchSeed));

            Vector3 startPosition = vertices[branchStartFromSegment + i + 1];

            // float randomX = Random.Range(-1.0f, 1.0f);
            // float randomZ = Random.Range(-1.0f, 1.0f);
            // Vector3 growthDirection = new Vector3(randomX, 1, randomZ).normalized;
            float minAngle = i * anglePerBranch + minAngleBetweenBranches / 2;
            float maxAngle = (i + 1) * anglePerBranch - minAngleBetweenBranches / 2;
            float randomAngle = Random.Range(minAngle, maxAngle);

            // 添加角度扰动
            float perturbation = Random.Range(-anglePerturbation, anglePerturbation);
            randomAngle += perturbation;

            float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float z = Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector3 growthDirection = new Vector3(x, 1, z).normalized;

            mainBranches[i].GenerateBranchVertices(startPosition, branchHeight, growthDirection, actualBranchGrowingFactor);

        }
    }

    private void UpdateLeafPositions(){

        allLeafPositions = new List<Vector3>();

        foreach (TreeBranch branch in mainBranches)
        {
            List<Vector3> branchLeafPositions = branch.GetLeafPositions(growingFactor);
            allLeafPositions.AddRange(branchLeafPositions);
        }

        // 对所有叶子按照Y轴从低到高进行排序
        allLeafPositions = allLeafPositions.OrderBy(position => position.y).ToList();

        // 根据 totalLeafCount 参数确定已经生长的叶子数量
        int actualLeafCount = Mathf.Min(totalLeafCount, allLeafPositions.Count);
        allLeafPositions = allLeafPositions.GetRange(0, actualLeafCount);
    }


    void Update()
    {

    }

    void OnValidate()
    {
        GenerateVertices();
        GenerateMainBranches();
        UpdateLeafPositions();

        Mesh trunkMesh = TreeMeshGenerator.CreateTrunkMesh(vertices, 0.2f, 8);
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = trunkMesh;
    }

    void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);

            if (i > 0)
            {
                Gizmos.DrawLine(vertices[i - 1], vertices[i]);
            }
        }

        Gizmos.color = Color.green;
        foreach (TreeBranch branch in mainBranches)
        {
            for (int i = 0; i < branch.vertices.Length; i++)
            {
                Gizmos.DrawSphere(branch.vertices[i], 0.1f);

                if (i > 0)
                {
                    Gizmos.DrawLine(branch.vertices[i - 1], branch.vertices[i]);
                }
            }
        }

        Gizmos.color = Color.yellow;
        foreach (Vector3 leafPosition in allLeafPositions)
        {
            Gizmos.DrawSphere(leafPosition, 0.2f);
        }

    }
}