using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace Items
{
    [RequireComponent(typeof(Collider))]
    public class InteractableItem : MonoBehaviour
    {
        [SerializeField] public string ItemName;
        [SerializeField][MinMaxRangeSlider(0.5f, 5)] public Vector2 radiusRange;
        [SerializeField] public string TextPath = "";

        string content = null;
        public void GetText(TextElement _text)
        {
            if (TextPath == "")
                _text.text = "";
            else if (content == null)
            {
                _text.text = "Downloading text...";
                WebUtil.GetTextFromServer(
                    TextPath,
                    (s) =>
                    {
                        content = s;
                        _text.text = s;
                    });

            }
            else
                _text.text = content;
        }
    }
}
