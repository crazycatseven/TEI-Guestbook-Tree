using UnityEngine;
using System;
using System.IO;
using System.Linq;

public class CustomLeafShape : MonoBehaviour
{

    [Range(0, 1)]
    public float controlPointX = 0.4f;

    [Range(0, 1)]
    public float controlPointY = 0.1f;


    [Range(0, 1)]
    public float controlPoint2X = 0.25f;

    [Range(0, 1)]
    public float controlPoint2Y = 0.7f;

    private float lastControlPoint2X;
    private float lastControlPoint2Y;

    public float leafHeight = 1f;

    [Range(0, 1f)]
    public float leafThickness = 0.25f;

    [Range(1, 100)]
    public int resolution = 10;

    [Range(0, 1)]
    public float leafHue = 0.26f; 

    [Range(0, 1)]
    public float leafSaturation = 0.75f; 

    [Range(0, 1)]
    public float leafBrightness = 0.7f;


    private Color leafColor;
    private float lastControlPointX;
    private float lastControlPointY;
    private float lastLeafHeight;
    private float lastLeafThickness;
    private int lastResolution;
    private float lastLeafHue;
    private float lastLeafSaturation;
    private float lastLeafBrightness;

    private MaterialPropertyBlock mpb;

    private void Start()
    {
        CreateLeafShape();
        lastControlPointX = controlPointX;
        lastControlPointY = controlPointY;
        lastControlPoint2X = controlPoint2X;
        lastControlPoint2Y = controlPoint2Y;
        lastLeafHeight = leafHeight;
        lastLeafThickness = leafThickness;
        lastResolution = resolution;
        lastLeafHue = leafHue;
        lastLeafSaturation = leafSaturation;
        lastLeafBrightness = leafBrightness;


        leafColor = Color.HSVToRGB(leafHue, leafSaturation, leafBrightness);

        // Create a new MaterialPropertyBlock
        mpb = new MaterialPropertyBlock();
        
        // Set color property
        mpb.SetColor("_Color", leafColor); 


        Renderer renderer = GetComponent<Renderer>();
        renderer.SetPropertyBlock(mpb);
    }

    private void Update()
    {

        if (controlPointX != lastControlPointX  || controlPointY != lastControlPointY  
            || controlPoint2X != lastControlPoint2X  || controlPoint2Y != lastControlPoint2Y 
            || leafHeight != lastLeafHeight || leafThickness != lastLeafThickness || resolution != lastResolution)
        {
            CreateLeafShape();
            lastControlPointX = controlPointX;
            lastControlPointY = controlPointY;
            lastControlPoint2X = controlPoint2X;
            lastControlPoint2Y = controlPoint2Y;
            lastLeafHeight = leafHeight;
            lastLeafThickness = leafThickness;
            lastResolution = resolution;
        }

        if (leafHue != lastLeafHue || leafSaturation != lastLeafSaturation || leafBrightness != lastLeafBrightness)
        {
            leafColor = Color.HSVToRGB(leafHue, leafSaturation, leafBrightness);

            // Create a new MaterialPropertyBlock
            mpb = new MaterialPropertyBlock();

            // Set color property
            mpb.SetColor("_Color", leafColor);  


            Renderer renderer = GetComponent<Renderer>();
            renderer.SetPropertyBlock(mpb);

            lastLeafHue = leafHue;
            lastLeafSaturation = leafSaturation;
            lastLeafBrightness = leafBrightness;
        }
    }

    private void CreateLeafShape()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Vector3[] vertices = new Vector3[resolution * 16];
        Vector2[] uv = new Vector2[resolution * 16];
        int[] triangles = new int[resolution * 24];


        Vector2 centerPoint = BezierCurve(0.5f);

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)resolution;
            Vector2 point = BezierCurve(t);
            float next_t = (i + 1) / (float)resolution;
            Vector2 next_point = BezierCurve(next_t);

            // Calculate distance from center
            float distanceFromCenter = Vector2.Distance(point, centerPoint);

            // Adjust leaf thickness based on distance from center

            float realLeafThickness = leafThickness * leafHeight * Mathf.Lerp(0.1f, 1f, (1 - Mathf.Abs(point.x) / leafHeight )) / 10;


            // Front face vertices
            vertices[i * 16] = new Vector3(point.y, point.x, -realLeafThickness / 2);
            vertices[i * 16 + 1] = new Vector3(-point.y, point.x, -realLeafThickness / 2);
            vertices[i * 16 + 2] = new Vector3(next_point.y, next_point.x, -realLeafThickness / 2);
            vertices[i * 16 + 3] = new Vector3(-next_point.y, next_point.x, -realLeafThickness / 2);

            // Back face vertices
            vertices[i * 16 + 4] = new Vector3(point.y, point.x, realLeafThickness / 2);
            vertices[i * 16 + 5] = new Vector3(-point.y, point.x, realLeafThickness / 2);
            vertices[i * 16 + 6] = new Vector3(next_point.y, next_point.x, realLeafThickness / 2);
            vertices[i * 16 + 7] = new Vector3(-next_point.y, next_point.x, realLeafThickness / 2);

            // Upper edge vertices
            vertices[i * 16 + 8] = new Vector3(point.y, point.x, -realLeafThickness / 2);
            vertices[i * 16 + 9] = new Vector3(next_point.y, next_point.x, -realLeafThickness / 2);
            vertices[i * 16 + 10] = new Vector3(point.y, point.x, realLeafThickness / 2);
            vertices[i * 16 + 11] = new Vector3(next_point.y, next_point.x, realLeafThickness / 2);

            // Lower edge vertices
            vertices[i * 16 + 12] = new Vector3(-point.y, point.x, -realLeafThickness / 2);
            vertices[i * 16 + 13] = new Vector3(-next_point.y, next_point.x, -realLeafThickness / 2);
            vertices[i * 16 + 14] = new Vector3(-point.y, point.x, realLeafThickness / 2);
            vertices[i * 16 + 15] = new Vector3(-next_point.y, next_point.x, realLeafThickness / 2);

            // Front face UV
            uv[i * 16] = new Vector2(0, t);                  // Bottom left
            uv[i * 16 + 1] = new Vector2(1, t);              // Bottom right
            uv[i * 16 + 2] = new Vector2(0, t + 1.0f / (resolution - 1));  // Top left
            uv[i * 16 + 3] = new Vector2(1, t + 1.0f / (resolution - 1));  // Top right

            // Back face UV
            uv[i * 16 + 4] = new Vector2(0, t);              // Bottom left
            uv[i * 16 + 5] = new Vector2(1, t);              // Bottom right
            uv[i * 16 + 6] = new Vector2(0, t + 1.0f / (resolution - 1));  // Top left
            uv[i * 16 + 7] = new Vector2(1, t + 1.0f / (resolution - 1));  // Top right


            // Upper edge UV
            uv[i * 16 + 8] = new Vector2(0.5f, t);           // Bottom
            uv[i * 16 + 9] = new Vector2(0.5f, t + 1.0f / (resolution - 1));  // Top
            uv[i * 16 + 10] = new Vector2(0.5f, t);          // Bottom (same as bottom left)
            uv[i * 16 + 11] = new Vector2(0.5f, t + 1.0f / (resolution - 1));  // Top (same as top left)

            // Lower edge UV
            uv[i * 16 + 12] = new Vector2(0.5f, t);          // Bottom
            uv[i * 16 + 13] = new Vector2(0.5f, t + 1.0f / (resolution - 1));  // Top
            uv[i * 16 + 14] = new Vector2(0.5f, t);          // Bottom (same as bottom left)
            uv[i * 16 + 15] = new Vector2(0.5f, t + 1.0f / (resolution - 1));  // Top (same as top left)


            // Front face triangles
            triangles[i * 24] = i * 16 + 1;
            triangles[i * 24 + 1] = i * 16 + 2;
            triangles[i * 24 + 2] = i * 16;

            triangles[i * 24 + 3] = i * 16 + 3;
            triangles[i * 24 + 4] = i * 16 + 2;
            triangles[i * 24 + 5] = i * 16 + 1;

            // Back face triangles
            triangles[i * 24 + 6] = i * 16 + 4;
            triangles[i * 24 + 7] = i * 16 + 6;
            triangles[i * 24 + 8] = i * 16 + 5;

            triangles[i * 24 + 9] = i * 16 + 5;
            triangles[i * 24 + 10] = i * 16 + 6;
            triangles[i * 24 + 11] = i * 16 + 7;

            // Upper edge triangles
            triangles[i * 24 + 12] = i * 16 + 8;
            triangles[i * 24 + 13] = i * 16 + 11;
            triangles[i * 24 + 14] = i * 16 + 10;

            triangles[i * 24 + 15] = i * 16 + 8;
            triangles[i * 24 + 16] = i * 16 + 9;
            triangles[i * 24 + 17] = i * 16 + 11;

            // Lower edge triangles
            triangles[i * 24 + 18] = i * 16 + 12;
            triangles[i * 24 + 19] = i * 16 + 14;
            triangles[i * 24 + 20] = i * 16 + 13;

            triangles[i * 24 + 21] = i * 16 + 13;
            triangles[i * 24 + 22] = i * 16 + 14;
            triangles[i * 24 + 23] = i * 16 + 15;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private Vector2 BezierCurve(float t)
    {
        Vector2 controlPoint1 = new Vector2(controlPointY * leafHeight, controlPointX * leafHeight);
        Vector2 controlPoint2 = new Vector2(controlPoint2Y * leafHeight, controlPoint2X * leafHeight);

        float u = 1 - t;
        Vector2 point = u * u * u * Vector2.zero;
        point += 3 * u * u * t * controlPoint1;
        point += 3 * u * t * t * controlPoint2;
        point += t * t * t * new Vector2(leafHeight, 0);
        return point;
    }


    public LeafData SaveLeafData(string path)
    {
        int id = 1;  // Default id
        if (File.Exists(path))
        {
            string lastLine = File.ReadLines(path).Last();  // Get last line of file
            if (!lastLine.StartsWith("id"))  // If last line is not the header
            {
                string[] values = lastLine.Split(',');
                id = int.Parse(values[0]) + 1;  // Get id from last line and increment it
            }
        }
        else  // If file doesn't exist, create it and add header
        {
            string header = "id,controlPointX,controlPointY,controlPoint2X,controlPoint2Y,leafHeight,leafThickness,resolution,leafHue,leafSaturation,leafBrightness,creationDate";
            File.AppendAllText(path, header + Environment.NewLine);
        }

        LeafData data = new LeafData
        {
            id = id,
            controlPointX = controlPointX,
            controlPointY = controlPointY,
            controlPoint2X = controlPoint2X,
            controlPoint2Y = controlPoint2Y,
            leafHeight = leafHeight,
            leafThickness = leafThickness,
            resolution = resolution,
            leafHue = leafHue,
            leafSaturation = leafSaturation,
            leafBrightness = leafBrightness,
            creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string line = $"{data.id},{data.controlPointX},{data.controlPointY},{data.controlPoint2X},{data.controlPoint2Y},{data.leafHeight},{data.leafThickness},{data.resolution},{data.leafHue},{data.leafSaturation},{data.leafBrightness},{data.creationDate}";
        File.AppendAllText(path, line + Environment.NewLine);  // Append line to file

        return data;
    }

    public void ResetToDefault()
    {
        controlPointX = 0.4f;
        controlPointY = 0.1f;
        controlPoint2X = 0.25f;
        controlPoint2Y = 0.7f;
        leafHeight = 1f;
        leafThickness = 0.25f;
        resolution = 10;
        leafHue = 0.26f;
        leafSaturation = 0.75f;
        leafBrightness = 0.7f;

        Start();
    }

}
