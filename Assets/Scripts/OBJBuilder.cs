using System.Collections.Generic;
using UnityEngine;

public class OBJBuilder
{
    public int pushedFaceCount { get; private set; } = 0;

    private OBJLoader loader;

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();

    private Dictionary<OBJLoopHasher, int> globalIndexRemap = new Dictionary<OBJLoopHasher, int>();
    private List<int> currentIndexList;

    //this will be set if the model has no normals or missing normal info
    private bool recalculateNormals = false;

    public OBJBuilder(OBJLoader loader)
    {
        this.loader = loader;
        currentIndexList = new List<int>();
    }

    public void PushFace(List<int> vertexIndices, List<int> normalIndices, List<int> uvIndices)
    {
        //Debug.Log("vertexIndices = " + vertexIndices.Count);
        if (vertexIndices.Count < 3)
        {
            return;
        }

        //remap
        int[] indexRemap = new int[vertexIndices.Count];
        for (int i = 0; i < vertexIndices.Count; i++)
        {
            int vertexIndex = vertexIndices[i];
            int normalIndex = normalIndices[i];
            int uvIndex = uvIndices[i];

            var hashObj = new OBJLoopHasher()
            {
                vertexIndex = vertexIndex,
                normalIndex = normalIndex,
                uvIndex = uvIndex
            };
            int remap = -1;

            if (!globalIndexRemap.TryGetValue(hashObj, out remap))
            {
                //add to dict
                globalIndexRemap.Add(hashObj, vertices.Count);
                remap = vertices.Count;

                //add new verts and what not
                vertices.Add((vertexIndex >= 0 && vertexIndex < loader.Vertices.Count) ? loader.Vertices[vertexIndex] : Vector3.zero);
                normals.Add((normalIndex >= 0 && normalIndex < loader.Normals.Count) ? loader.Normals[normalIndex] : Vector3.zero);
                uvs.Add((uvIndex >= 0 && uvIndex < loader.UVs.Count) ? loader.UVs[uvIndex] : Vector2.zero);

                //mark recalc flag
                if (normalIndex < 0)
                    recalculateNormals = true;
            }

            indexRemap[i] = remap;
        }

        //add face to our mesh list
        if (indexRemap.Length == 3)
        {
            currentIndexList.AddRange(new int[] { indexRemap[0], indexRemap[1], indexRemap[2] });
        }

        else if (indexRemap.Length == 4)
        {
            currentIndexList.AddRange(new int[] { indexRemap[0], indexRemap[1], indexRemap[2] });
            currentIndexList.AddRange(new int[] { indexRemap[2], indexRemap[3], indexRemap[0] });
        }

        else if (indexRemap.Length > 4)
        {
            for (int i = indexRemap.Length - 1; i >= 2; i--)
            {
                currentIndexList.AddRange(new int[] { indexRemap[0], indexRemap[i - 1], indexRemap[i] });
            }
        }

        pushedFaceCount++;
    }

    public GameObject Build()
    {
        var gameObject = new GameObject("obj");

        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = CreateNullMaterial();

        var meshFilter = gameObject.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        mesh.name = "mesh";
        mesh.indexFormat = (vertices.Count > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.subMeshCount = 1;

        //set vertex data
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(currentIndexList, 0);

        //foreach(int i in currentIndexList)
        //{
        //    Debug.Log(i);
        //}

        Debug.Log("vertices  = " + vertices.Count);
        Debug.Log("normals  = " + normals.Count);
        Debug.Log("uvs  = " + uvs.Count);
        Debug.Log("triangles  = " + currentIndexList.Count);

        //for (int i = 0; i < uvs.Count; i++)
        //{
        //    Debug.Log(uvs[i].x + "  " + uvs[i].y);
        //}

        if (recalculateNormals)
            mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();


        foreach (Vector3 normal in mesh.normals)
        {
            Debug.Log(normal);
        }

        //foreach (int tri in mesh.triangles)
        //{
        //    Debug.Log(tri);
        //}

        //Debug.Log("before " + normals[0]);
        //Debug.Log("after " + mesh.normals[0]);

        meshFilter.sharedMesh = mesh;

        return gameObject;
    }

    private Material CreateNullMaterial()
    {
        return Resources.Load("New_Material") as Material;
        //return new Material(Shader.Find("Standard (Specular setup)"));
    }
}

class OBJLoopHasher
{
    public int vertexIndex;
    public int normalIndex;
    public int uvIndex;

    public override bool Equals(object obj)
    {
        if (!(obj is OBJLoopHasher))
            return false;

        var hash = obj as OBJLoopHasher;
        return (hash.vertexIndex == vertexIndex) && (hash.uvIndex == uvIndex) && (hash.normalIndex == normalIndex);
    }

    public override int GetHashCode()
    {
        int hc = 3;
        hc = unchecked(hc * 314159 + vertexIndex);
        hc = unchecked(hc * 314159 + normalIndex);
        hc = unchecked(hc * 314159 + uvIndex);
        return hc;
    }
}