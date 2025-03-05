using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    [UxmlElement]
    public partial class CrosshairImage : VisualElement
    {
        static CrosshairImage instance;
        static VisualTreeAsset document = null;
        public CrosshairImage()
        {
            Add(new());
            instance = this;
        }


        public static void Enter()
            => instance.AddToClassList("Active");
        public static void Exit()
        {
            instance.RemoveFromClassList("Active");
            EndHold();
        }

        public static void StartHold()
            => instance.AddToClassList("Holding");
        public static void EndHold()
            => instance.RemoveFromClassList("Holding");

        public static void Toggle()
        {
            if(document == null)
            {
                EndHold();
                document = GameObject.Find("UI").GetComponent<UIDocument>().visualTreeAsset;
                GameObject.Find("UI").GetComponent<UIDocument>().visualTreeAsset = null;
            }
            else
            {
                GameObject.Find("UI").GetComponent<UIDocument>().visualTreeAsset = document;
                document = null;
            }
        }
    }
}