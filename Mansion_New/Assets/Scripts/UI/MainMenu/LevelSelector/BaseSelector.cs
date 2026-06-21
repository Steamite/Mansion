using Assets.Scripts.Interactable_Items.Rooms;
using Assets.Scripts.UI.MainMenu.SceneLoader;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Assets.Scripts.UI.MainMenu.New
{
    public class BaseSelector : MonoBehaviour
    {
        public UIDocument document;
        public BaseLoadingScreen loadingScreen;


        [SerializeField] PanelSettings screenSettings;
        [SerializeField] PanelSettings worldSettings;

        [SerializeField] GameObject mainCamera;

        protected virtual void ShowUI()
        {
            if (loadingScreen.UseVR)
            {
                mainCamera.SetActive(false);

                document.enabled = false;
                document.panelSettings = worldSettings;
                //document.worldSpaceSize = new(1920, 1080); TEST
                document.enabled = true;

                UIDocument doc = loadingScreen.transform.parent.GetComponent<UIDocument>();
                doc.enabled = true;
                doc.panelSettings = worldSettings;
                doc.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
                doc.enabled = false;

                AddressableSceneManager.LoadScene(
                    "Player VR",
                    SceneType.Player,
                    null,
                    (scene) =>
                    {
                        GameObject[] objs = scene.Scene.GetRootGameObjects();
                        Camera.main.cullingMask = LayerMask.GetMask("UI", "Ignore Raycast");// LayerMask.NameToLayer("UI");
                    });
            }
            else
            {
                mainCamera.SetActive(true);

                document.panelSettings = screenSettings;
                loadingScreen.transform.parent.GetComponent<UIDocument>().panelSettings = screenSettings;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
