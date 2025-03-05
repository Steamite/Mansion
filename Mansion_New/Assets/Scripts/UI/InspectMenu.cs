using Items;
using Player;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI.Inspect
{
    public class InspectMenu : MonoBehaviour
    {
        const string DESCRIPTION = "Description-Label";
        const string DESCRIPTIONOPTION = "Description-Option";
        const string TITLE = "Title-Label";

        [SerializeField] CinemachineCamera cam;
        InputActionAsset asset;

        InputAction endAction;
        InputAction infoAction;
        InputAction takeAction;

        InteractableItem item;

        UIDocument doc;
        bool isDescriptionOpened;

        #region INIT
        public void Init(InputActionAsset _asset, InteractableItem _item)
        {
            asset = _asset;
            item = _item;

            endAction = asset.actionMaps[2].actions[0];
            infoAction = asset.actionMaps[2].actions[1];
            takeAction = asset.actionMaps[2].actions[2];
            asset.actionMaps[2].Enable();

            doc = GetComponent<UIDocument>();
            doc.enabled = true;
            doc.rootVisualElement.Q<Label>(TITLE).text = _item.ItemName;

            if(item.TextPath == "")
            {
                infoAction.Disable();
                doc.rootVisualElement.Q<VisualElement>(DESCRIPTIONOPTION).style.display = DisplayStyle.None;
            }
            isDescriptionOpened = false;


            enabled = true;
        }
        #endregion

        void Update()
        {
            if (endAction.triggered)
                EndInteract();
            else if (infoAction.triggered)
                InfoToggle();
            else if (!isDescriptionOpened && takeAction.triggered)
                PickupItem();
        }

        #region End
        void EndInteract()
        {
            asset.actionMaps[2].Disable();
            if (item)
            {
                foreach (Transform trans in item.GetComponentsInChildren<Transform>(true))
                    trans.gameObject.layer = 7;
                item.transform.SetParent(GameObject.Find("World").transform);
            }
            Camera.main.transform.SetParent(GameObject.Find("UI").transform);
            Camera.main.transform.SetParent(null);

            doc.enabled = false;
            cam.Priority = -1;
            StartCoroutine(WaitForBlend());
        }

        IEnumerator WaitForBlend()
        {
            CrosshairImage.Toggle();
            yield return new();
            Camera.main.cullingMask = -1;
            gameObject.SetActive(false);
            SceneManager.UnloadSceneAsync(1);
            asset.actionMaps[0].Enable();
            GameObject.FindFirstObjectByType<PlayerCamera>().EndIteract();
        }
        #endregion

        #region Descriptions
        void InfoToggle()
        {
            if (!isDescriptionOpened)
            {
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                cam.GetComponent<CinemachineInputAxisController>().enabled = false;

                doc.rootVisualElement.AddToClassList("Description");
                doc.rootVisualElement.RemoveFromClassList("Inspect");
                
                ((Label)doc.rootVisualElement.Q<VisualElement>(DESCRIPTIONOPTION).ElementAt(2)).text = "Zavřít popis";
                item.GetText(doc.rootVisualElement.Q<Label>(DESCRIPTION));

                isDescriptionOpened = true;
                endAction.Disable();
                takeAction.Disable();
            }
            else
            {
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                cam.GetComponent<CinemachineInputAxisController>().enabled = true;

                doc.rootVisualElement.RemoveFromClassList("Description");
                doc.rootVisualElement.AddToClassList("Inspect");
                
                doc.rootVisualElement.Q<Label>(DESCRIPTION).text = "";
                ((Label)doc.rootVisualElement.Q<VisualElement>(DESCRIPTIONOPTION).ElementAt(2)).text = "Popis";
                
                isDescriptionOpened = false;
                endAction.Enable();
                takeAction.Enable();
            }
        }

        #endregion
        void PickupItem()
        {
            Destroy(item.gameObject);
            item = null;
            EndInteract();
        }
    }
}