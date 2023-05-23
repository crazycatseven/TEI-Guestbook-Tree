using UnityEngine;

public class CustomLeafShape : MonoBehaviour
{
    public Vector2 controlPoint = new Vector2(0.5f, 0.5f);
    public float offset = 0.005f;  // 用于微调顶部和底部的参数，可以根据需要进行调整


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
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        float thickness = 0.1f;
        Vector3[] vertices = new Vector3[resolution * 16];
        int[] triangles = new int[resolution * 24];

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)resolution;
            Vector2 point = BezierCurve(t);
            float next_t = (i + 1) / (float)resolution;
            Vector2 next_point = BezierCurve(next_t);

            // Front face vertices
            vertices[i * 16] = new Vector3(point.x, point.y);
            vertices[i * 16 + 1] = new Vector3(point.x, -point.y);
            vertices[i * 16 + 2] = new Vector3(next_point.x, next_point.y);
            vertices[i * 16 + 3] = new Vector3(next_point.x, -next_point.y);

            // Back face vertices
            vertices[i * 16 + 4] = new Vector3(point.x, point.y, thickness);
            vertices[i * 16 + 5] = new Vector3(point.x, -point.y, thickness);
            vertices[i * 16 + 6] = new Vector3(next_point.x, next_point.y, thickness);
            vertices[i * 16 + 7] = new Vector3(next_point.x, -next_point.y, thickness);

            // Upper edge vertices
            vertices[i * 16 + 8] = new Vector3(point.x, point.y);
            vertices[i * 16 + 9] = new Vector3(next_point.x, next_point.y);
            vertices[i * 16 + 10] = new Vector3(point.x, point.y, thickness);
            vertices[i * 16 + 11] = new Vector3(next_point.x, next_point.y, thickness);

            // Lower edge vertices
            vertices[i * 16 + 12] = new Vector3(point.x, -point.y);
            vertices[i * 16 + 13] = new Vector3(next_point.x, -next_point.y);
            vertices[i * 16 + 14] = new Vector3(point.x, -point.y, thickness);
            vertices[i * 16 + 15] = new Vector3(next_point.x, -next_point.y, thickness);

            // Front face triangles
            triangles[i * 24] = i * 16;
            triangles[i * 24 + 1] = i * 16 + 2;
            triangles[i * 24 + 2] = i * 16 + 1;

            triangles[i * 24 + 3] = i * 16 + 1;
            triangles[i * 24 + 4] = i * 16 + 2;
            triangles[i * 24 + 5] = i * 16 + 3;

            // Back face triangles
            triangles[i * 24 + 6] = i * 16 + 5;
            triangles[i * 24 + 7] = i * 16 + 6;
            triangles[i * 24 + 8] = i * 16 + 4;

            triangles[i * 24 + 9] = i * 16 + 7;
            triangles[i * 24 + 10] = i * 16 + 6;
            triangles[i * 24 + 11] = i * 16 + 5;

            // Upper edge triangles
            triangles[i * 24 + 12] = i * 16 + 10;
            triangles[i * 24 + 13] = i * 16 + 11;
            triangles[i * 24 + 14] = i * 16 + 8;

            triangles[i * 24 + 15] = i * 16 + 11;
            triangles[i * 24 + 16] = i * 16 + 9;
            triangles[i * 24 + 17] = i * 16 + 8;

            // Lower edge triangles
            triangles[i * 24 + 18] = i * 16 + 13;
            triangles[i * 24 + 19] = i * 16 + 14;
            triangles[i * 24 + 20] = i * 16 + 12;

            triangles[i * 24 + 21] = i * 16 + 15;
            triangles[i * 24 + 22] = i * 16 + 14;
            triangles[i * 24 + 23] = i * 16 + 13;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    // private Vector2 BezierCurve(float t)
    // {
    //     float u = 1 - t;
    //     Vector2 point = u * u * Vector2.zero;
    //     point += 2 * u * t * controlPoint;
    //     point += t * t * new Vector2(leafHeight, 0);

    //     return point;
    // }

    private Vector2 BezierCurve(float t)
    {
        float u = 1 - t;
        Vector2 point = u * u * Vector2.zero;
        point += 2 * u * t * controlPoint;
        point += t * t * new Vector2(leafHeight, 0);
        return point;
    }

}
