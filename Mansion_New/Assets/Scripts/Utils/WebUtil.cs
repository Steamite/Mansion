using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static System.Collections.Specialized.BitVector32;

public class WebUtil : MonoBehaviour
{
    static WebUtil instance = null;
	[RuntimeInitializeOnLoadMethod]
	static void Refresh() =>
		instance = null;
    private void Awake()
    {
        instance = this;
    }

	public static void CancelDownloads()
	{
		instance.StopAllCoroutines();
	}

	#region Text
	public static void GetTextFromServer(string path, Action<string> action)
    {
        if (instance == null)
            throw new NotImplementedException("Add webUtil to scene");
        instance.StartCoroutine(instance.DownloadText(path, action));
    }
    IEnumerator DownloadText(string path, Action<string> action)
    {
        for (int i = 0; i < 3; i++)
        {
			using UnityWebRequest request = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, path));

			request.timeout = 2;
			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
                if(i == 2)
				    action("ERROR");
			}
			else
			{
				action(request.downloadHandler.text);
				i = 3;
			}
		}
    }
	#endregion

	#region PDF
	public static void GetImageFromServer(string path, Action<Texture2D> action)
	{
		if (instance == null)
			throw new NotImplementedException("Add webUtil to scene");
		instance.StartCoroutine(instance.DownloadImage(path, action));
	}

	IEnumerator DownloadImage(string path, Action<Texture2D> action)
	{
		for (int i = 0; i < 3; i++)
		{
			using UnityWebRequest request = UnityWebRequestTexture.GetTexture(Path.Combine(Application.streamingAssetsPath, path));

			request.timeout = 2;
			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				if (i == 2)
					action(null);
			}
			else
			{
				action(DownloadHandlerTexture.GetContent(request));
				i = 3;
			}
		}
	}

	#endregion
}
