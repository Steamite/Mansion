using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    [UxmlElement]
    public partial class CrosshairImage : VisualElement
    {
        static CrosshairImage instance;
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
            instance.ToggleInClassList("Disabled");
            if(instance.ClassListContains("Active"))
                Exit();
        }
    }
}