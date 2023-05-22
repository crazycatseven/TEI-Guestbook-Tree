using UnityEngine;

public class CustomLeafShape : MonoBehaviour
{
    public Vector2 controlPoint = new Vector2(0.5f, 0.5f);
    public float leafHeight = 1f;
    public int resolution = 100;

    private Vector2 lastControlPoint;
    private float lastLeafHeight;

    private void Start()
    {
        CreateLeafShape();
        lastControlPoint = controlPoint;
        lastLeafHeight = leafHeight;
    }

    private void Update()
    {
        if (controlPoint != lastControlPoint || leafHeight != lastLeafHeight)
        {
            CreateLeafShape();
            lastControlPoint = controlPoint;
            lastLeafHeight = leafHeight;
        }
    }

    private void CreateLeafShape()
    {
        // Create the Mesh
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Vector3[] vertices = new Vector3[(resolution + 1) * 2];
        int[] triangles = new int[resolution * 6];

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector2 point = BezierCurve(t);
            vertices[i * 2] = new Vector3(point.x, point.y);
            vertices[i * 2 + 1] = new Vector3(point.x, -point.y);

            if (i != resolution)
            {
                triangles[i * 6] = i * 2;
                triangles[i * 6 + 1] = i * 2 + 1;
                triangles[i * 6 + 2] = i * 2 + 2;

                triangles[i * 6 + 3] = i * 2 + 1;
                triangles[i * 6 + 4] = i * 2 + 3;
                triangles[i * 6 + 5] = i * 2 + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    private Vector2 BezierCurve(float t)
    {
        float u = 1 - t;
        Vector2 point = u * u * Vector2.zero;
        point += 2 * u * t * controlPoint;
        point += t * t * new Vector2(leafHeight, 0);

        return point;
    }
}
