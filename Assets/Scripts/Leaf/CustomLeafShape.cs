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
    public float leafThickness = 0.5f;

    [Range(1, 100)]
    public int resolution = 10;

    [Range(0, 1)]
    public float leafHue = 0.3f; // 绿色的色相约为0.3

    [Range(0, 1)]
    public float leafSaturation = 0.8f; // 绿色的饱和度约为0.8

    [Range(0, 1)]
    public float leafBrightness = 1f; // 明度默认为1


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

        // 创建一个新的材质属性块
        mpb = new MaterialPropertyBlock();
        
        // 设置颜色属性
        mpb.SetColor("_Color", leafColor);  // 你想要的颜色

        // 获取Renderer组件并应用材质属性块
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

            // 创建一个新的材质属性块
            mpb = new MaterialPropertyBlock();

            // 设置颜色属性
            mpb.SetColor("_Color", leafColor);  // 你想要的颜色

            // 获取Renderer组件并应用材质属性块
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
        int[] triangles = new int[resolution * 24];

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)resolution;
            Vector2 point = BezierCurve(t);
            float next_t = (i + 1) / (float)resolution;
            Vector2 next_point = BezierCurve(next_t);

            float realLeafThickness = leafThickness /10;

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
        int id = 1;  // 默认id为1
        if (File.Exists(path))
        {
            string lastLine = File.ReadLines(path).Last();  // 读取文件的最后一行
            if (!lastLine.StartsWith("id"))  // 如果最后一行不是头部行
            {
                string[] values = lastLine.Split(',');
                id = int.Parse(values[0]) + 1;  // 获取最后一行的id并加一
            }
        }
        else  // 如果文件不存在，需要添加一个头部行
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
        File.AppendAllText(path, line + Environment.NewLine);  // 在文件末尾添加新的数据行

        return data;
    }


}
