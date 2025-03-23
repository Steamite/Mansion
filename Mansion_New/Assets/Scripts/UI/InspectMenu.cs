using Items;
using Player;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI.Inspect
{
    /// <summary>Handles input for the input menu.</summary>
    public class InspectMenu : MonoBehaviour
    {
		#region Variables
		/// <summary>Path to the Desctiption scrollview.</summary>
		public const string DESCRIPTION = "Description";
        /// <summary>Path to description "Button".</summary>
        const string DESCRIPTION_OPTION = "D";
        /// <summary>Title label element.</summary>
        const string TITLE = "Title-Label";

        /// <summary>Inpect camera.</summary>
		[SerializeField] CinemachineCamera cam;
        /// <summary>Input holder.</summary>
        InputActionAsset asset;

        /// <summary>Ends interaction.</summary>
        InputAction endAction;
        /// <summary>Toggles interaction.</summary>
		InputAction infoAction;
        /// <summary>Destoys the inpected item.</summary>
		InputAction takeAction;

        /// <summary>Item in inspection.</summary>
		InteractableItem item;

        /// <summary>Root of the document.</summary>
		UIDocument doc;
        /// <summary>Is the description page opened or not.</summary>
		bool isDescriptionOpened;
		#endregion

		#region Init
		/// <summary>
		/// Maps actions, and hides the description option if no file is assigned to the inspected item.
		/// </summary>
		/// <param name="_asset">Input asset with inpection map.</param>
		/// <param name="_item">Inspected item.</param>
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

            if(item.sourceObject.IsValid())
            {
                infoAction.Disable();
                doc.rootVisualElement.Q<VisualElement>(DESCRIPTION_OPTION).style.display = DisplayStyle.None;
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
                DescriptionToggle();
            /*else if (!isDescriptionOpened && takeAction.triggered)
                PickupItem();*/
            else if (takeAction.triggered && isDescriptionOpened && item is PDFItem)
				Application.OpenURL(((PDFItem)item).pdfPath);
		}

        #region End
        /// <summary>
        /// Ends the interaction.
        /// </summary>
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

        /// <summary>
        /// Unloads interaction scene and resets player camera.
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitForBlend()
        {
            CrosshairImage.Toggle();
            yield return new();
            Camera.main.cullingMask = -1;
            gameObject.SetActive(false);
			//AsyncOperationHandle<SceneInstance> sceneUnload = Addressables.UnloadSceneAsync();
            SceneManager.UnloadSceneAsync("Interact");
            asset.actionMaps[0].Enable();
            GameObject.FindFirstObjectByType<PlayerCamera>().EndIteract();
        }
        #endregion

        #region Descriptions
        /// <summary>
        /// Opens or closes the description
        /// </summary>
        void DescriptionToggle()
        {
            if (!isDescriptionOpened)
            {
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                cam.GetComponent<CinemachineInputAxisController>().enabled = false;

                doc.rootVisualElement.AddToClassList("Description");
                doc.rootVisualElement.RemoveFromClassList("Inspect");
                
                ((Label)doc.rootVisualElement.Q<VisualElement>(DESCRIPTION_OPTION).ElementAt(2)).text = "Zavřít popis";
                
                item.LoadContent(doc.rootVisualElement.Q<ScrollView>(DESCRIPTION)
                    .Q<VisualElement>("unity-content-container"));

                isDescriptionOpened = true;
                endAction.Disable();
            }
            else
            {
                WebUtil.CancelDownloads();
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                cam.GetComponent<CinemachineInputAxisController>().enabled = true;

                doc.rootVisualElement.RemoveFromClassList("Description");
                doc.rootVisualElement.AddToClassList("Inspect");

				item.Unload(doc.rootVisualElement.Q<ScrollView>(DESCRIPTION)
					.Q<VisualElement>("unity-content-container"));

				((Label)doc.rootVisualElement.Q<VisualElement>(DESCRIPTION_OPTION).ElementAt(2)).text = "Popis";
                
                isDescriptionOpened = false;
                endAction.Enable();
            }
        }

        #endregion
        /// <summary>
        /// Destroys the item and ends interaction.
        /// </summary>
        void PickupItem()
        {
            if(!isDescriptionOpened){
                Destroy(item.gameObject);
                item = null;
                EndInteract();
            }
        }
    }
}