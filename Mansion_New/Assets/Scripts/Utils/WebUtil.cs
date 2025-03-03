using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebUtil : MonoBehaviour
{
    static WebUtil instance;
    private void Awake()
    {
        instance = this;
    }
    public static void GetTextFromServer(string path, Action<string> action)
    {
        if (instance == null)
            throw new NotImplementedException("Add webUtil to scene");
        instance.StartCoroutine(instance.DownloadText(path, action));
    }

    IEnumerator DownloadText(string path, Action<string> action)
    {
        using UnityWebRequest request = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, path));
        
        request.timeout = 2;
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            action(request.result.ToString());
        }
        else
        {
            action(request.downloadHandler.text);
        }
    }
}
