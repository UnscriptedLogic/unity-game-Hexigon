using System;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public struct Face
{
    public List<Vector3> vertices { get; private set; }
    public List<int> triangles { get; private set; }
    public List<Vector2> uvs { get; private set; }

    public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
}

[System.Serializable]
public struct HexData
{
    public float innerRadius;
    public float outerRadius;
    public float height;
    public bool isPointyTop;
    public Material material;
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HexRenderer : MonoBehaviour
{
    public class CreateFaceArgs
    {
        public float innerRadius;
        public float outerRadius;
        public float heightA;
        public float heightB;
        public int point;
        public bool isReversed = false;
    }

    [Header("Settings")]
    [SerializeField] private HexData hexData;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private List<Face> faces;

    public void InitializeMesh(HexData hexData)
    {
        this.hexData = hexData;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.name = "Hex";

        meshFilter.mesh = mesh;
        meshRenderer.material = hexData.material;

        DrawMesh();
    }


    private void DrawMesh()
    {
        DrawFaces();
        CombineFaces();
    }

    private void DrawFaces()
    {
        faces = new List<Face>();

        for (int point = 0; point < 6; point++)
        {
            faces.Add(CreateFace(new CreateFaceArgs()
            {
                innerRadius = hexData.innerRadius,
                outerRadius = hexData.outerRadius,
                heightA = hexData.height / 2f,
                heightB = hexData.height / 2f,
                point = point,
                isReversed = false
            }));

            faces.Add(CreateFace(new CreateFaceArgs()
            {
                innerRadius = hexData.innerRadius,
                outerRadius = hexData.outerRadius,
                heightA = -hexData.height / 2f,
                heightB = -hexData.height / 2f,
                point = point,
                isReversed = true
            }));

            faces.Add(CreateFace(new CreateFaceArgs()
            {
                innerRadius = hexData.outerRadius,
                outerRadius = hexData.outerRadius,
                heightA = hexData.height / 2f,
                heightB = -hexData.height / 2f,
                point = point,
                isReversed = true
            }));

            faces.Add(CreateFace(new CreateFaceArgs()
            {
                innerRadius = hexData.innerRadius,
                outerRadius = hexData.innerRadius,
                heightA = hexData.height / 2f,
                heightB = -hexData.height / 2f,
                point = point,
                isReversed = false
            }));
        }
    }

    private void CombineFaces()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < faces.Count; i++)
        {
            vertices.AddRange(faces[i].vertices);
            uvs.AddRange(faces[i].uvs);

            int offset = (4 * i);
            foreach (int triangle in faces[i].triangles)
            {
                triangles.Add(triangle + offset);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }

    private Face CreateFace(CreateFaceArgs args)
    {
        Vector3 pointA = GetPoint(args.innerRadius, args.heightB, args.point);
        Vector3 pointB = GetPoint(args.innerRadius, args.heightB, (args.point < 5) ? args.point + 1 : 0);
        Vector3 pointC = GetPoint(args.outerRadius, args.heightA, (args.point < 5) ? args.point + 1 : 0);
        Vector3 pointD = GetPoint(args.outerRadius, args.heightA, args.point);

        List<Vector3> vertices = new List<Vector3>() { pointA, pointB, pointC, pointD };
        List<int> triangles = new List<int>() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new List<Vector2>() { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };

        if (args.isReversed)
        {
            vertices.Reverse();
        }

        return new Face(vertices, triangles, uvs);
    }

    private Vector3 GetPoint(float size, float height, int index)
    {
        float angleDeg = 60 * index;

        if (hexData.isPointyTop) angleDeg -= 30f;
        
        float angleRad = Mathf.PI / 180f * angleDeg;
        return new Vector3((size * Mathf.Cos(angleRad)), height, size * Mathf.Sin(angleRad));
    }
}
