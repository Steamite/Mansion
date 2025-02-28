using System.IO;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Networking;

namespace Items
{
    [RequireComponent(typeof(Collider))]
    public class InteractableItem : MonoBehaviour
    {
        [SerializeField] public string ItemName;
        [SerializeField][MinMaxRangeSlider(0.5f, 5)] public Vector2 radiusRange;
        [SerializeField] public string TextPath = "";

        string text;
        [SerializeField] public string Text 
        {
            get
            {
                if (TextPath == "")
                    return "";
                if (text == null)
                    text = WebUtil.DowloadText(TextPath).Result;
                return text;
            }
        }
    }

}
