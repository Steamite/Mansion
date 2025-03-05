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
        instance.StartCoroutine(instance.DownloadText(path, action, 0));
    }

    IEnumerator DownloadText(string path, Action<string> action, int tryI)
    {
        using UnityWebRequest request = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, path));
        
        request.timeout = 2;
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            if(tryI < 3)
                StartCoroutine(DownloadText(path, action, tryI++));
            else
                action("ERROR");
        }
        else
        {
            action(request.downloadHandler.text);
        }
    }
}
