using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OBJFromWeb : MonoBehaviour
{
    void Start()
    {
        _ = StartCoroutine(FetchObj());
    }

    IEnumerator FetchObj()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://people.sc.fsu.edu/~jburkardt/data/obj/lamp.obj");
        yield return request.SendWebRequest();

        var textStream = new MemoryStream(Encoding.UTF8.GetBytes(request.downloadHandler.text));
        _ = new OBJLoader().Load(textStream);
    }
}
