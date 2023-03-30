using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OBJLoader
{
    //global lists, accessed by objobjectbuilder
    internal List<Vector3> Vertices = new List<Vector3>();
    internal List<Vector3> Normals = new List<Vector3>();
    internal List<Vector2> UVs = new List<Vector2>();

    public GameObject Load(string filePath)
    {
        using FileStream fileStream = File.OpenRead(filePath);
        return ReadObjText(fileStream);
    }

    public GameObject Load(Stream input)
    {
        return ReadObjText(input);
    }

    private GameObject ReadObjText(Stream input)
    {
        var reader = new StreamReader(input);

        OBJBuilder builder = new OBJBuilder(this);

        //lists for face data
        List<int> vertexIndices = new List<int>();
        List<int> normalIndices = new List<int>();
        List<int> uvIndices = new List<int>();

        var buffer = new LineReader(reader, 4 * 1024);

        while (true)
        {
            buffer.SkipWhitespaces();

            if (buffer.endReached)
            {
                break;
            }

            buffer.ReadUntilWhiteSpace();

            if (buffer.Is("#"))
            {
                buffer.SkipUntilNewLine();
                continue;
            }

            if (buffer.Is("v"))
            {
                Vertices.Add(buffer.ReadVector());
                continue;
            }

            //normal
            if (buffer.Is("vn"))
            {
                Normals.Add(buffer.ReadVector());
                continue;
            }

            //uv
            if (buffer.Is("vt"))
            {
                UVs.Add(buffer.ReadVector());
                continue;
            }

            //face data
            if (buffer.Is("f"))
            {
                //loop through indices
                while (true)
                {
                    bool newLinePassed;
                    buffer.SkipWhitespaces(out newLinePassed);
                    if (newLinePassed == true)
                    {
                        break;
                    }

                    int vertexIndex = int.MinValue;
                    int normalIndex = int.MinValue;
                    int uvIndex = int.MinValue;

                    vertexIndex = buffer.ReadInt();
                    if (buffer.currentChar == '/')
                    {
                        buffer.MoveNext();
                        if (buffer.currentChar != '/')
                        {
                            uvIndex = buffer.ReadInt();
                        }
                        if (buffer.currentChar == '/')
                        {
                            buffer.MoveNext();
                            normalIndex = buffer.ReadInt();
                        }
                    }

                    //"postprocess" indices
                    if (vertexIndex > int.MinValue)
                    {
                        if (vertexIndex < 0)
                            vertexIndex = Vertices.Count - vertexIndex;
                        vertexIndex--;
                    }
                    if (normalIndex > int.MinValue)
                    {
                        if (normalIndex < 0)
                            normalIndex = Normals.Count - normalIndex;
                        normalIndex--;
                    }
                    if (uvIndex > int.MinValue)
                    {
                        if (uvIndex < 0)
                            uvIndex = UVs.Count - uvIndex;
                        uvIndex--;
                    }

                    //set array values
                    vertexIndices.Add(vertexIndex);
                    normalIndices.Add(normalIndex);
                    uvIndices.Add(uvIndex);
                }

                //push to builder
                builder.PushFace(vertexIndices, normalIndices, uvIndices);

                //clear lists
                vertexIndices.Clear();
                normalIndices.Clear();
                uvIndices.Clear();

                continue;
            }

            buffer.SkipUntilNewLine();
        }

        GameObject obj = new GameObject("obj");
        obj.transform.localScale = new Vector3(-1f, 1f, 1f);

        var builtGameObject = builder.Build();
        builtGameObject.transform.SetParent(obj.transform, false);

        return obj;
    }
}