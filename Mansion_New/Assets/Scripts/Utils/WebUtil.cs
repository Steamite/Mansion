using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class WebUtil
{
    public static async Task<string> DowloadText(string path)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, path)))
        {
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                return request.result.ToString();
            }
            else
            {
                return request.downloadHandler.text;
            }
        }
    }
}
