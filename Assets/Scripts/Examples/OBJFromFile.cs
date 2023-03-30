using System.IO;
using UnityEngine;

public class OBJFromFile : MonoBehaviour
{
    string objPath = string.Empty;
    string error = string.Empty;
    GameObject wrapMesh;
    GameObject classifyWrapped;

    void OnGUI()
    {
        objPath = GUI.TextField(new Rect(25, 100, 400, 98), objPath);
        if (GUI.Button(new Rect(600, 100, 120, 98), "Load File"))
        {
            if (!Directory.Exists(objPath))
            {
                error = "File doesn't exist.";
            }
            else
            {
                if (wrapMesh != null)
                {
                    Destroy(wrapMesh);
                }

                if (classifyWrapped != null)
                {
                    Destroy(classifyWrapped);
                }

                wrapMesh = new OBJLoader().Load(objPath + "wrapMesh.obj");
                wrapMesh.transform.Rotate(0, 180, 0);

                //classifyWrapped = new OBJLoader().Load(objPath + "classifyWrapped.obj");
                //classifyWrapped.transform.Translate(0, -35, 0);
                //classifyWrapped.transform.Rotate(0, 180, 0);

                error = string.Empty;
            }
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            GUI.color = Color.red;
            GUI.Box(new Rect(25, 64, 256 + 64, 32), error);
            GUI.color = Color.white;
        }
    }
}