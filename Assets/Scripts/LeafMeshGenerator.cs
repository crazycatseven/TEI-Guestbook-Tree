using UnityEngine;

public class LeafMeshGenerator
{
    public static Mesh CreateLeafMesh(Vector2 leafSize)
    {
        Mesh leafMesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-leafSize.x / 2, 0, -leafSize.y / 2),
            new Vector3(leafSize.x / 2, 0, -leafSize.y / 2),
            new Vector3(-leafSize.x / 2, 0, leafSize.y / 2),
            new Vector3(leafSize.x / 2, 0, leafSize.y / 2),
        };

        int[] triangles = new int[6] { 0, 2, 1, 2, 3, 1 };

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };

        leafMesh.vertices = vertices;
        leafMesh.triangles = triangles;
        leafMesh.uv = uv;
        leafMesh.RecalculateNormals();

        return leafMesh;
    }
}
