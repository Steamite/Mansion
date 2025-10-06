using Items;
using NUnit.Framework.Constraints;
using Player;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UnityEngine.Rendering;
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
        const string HOME_OPTION = "Home";
        /// <summary>Title label element.</summary>
        const string TITLE = "Title-Label";

        /// <summary>Inpect camera.</summary>
		[SerializeField] CinemachineCamera cam;
        /// <summary>Input holder.</summary>
        public InputActionAsset asset;

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
		public bool isDescriptionOpened;


        AsyncOperationHandle<SceneInstance> scene;
        const string buttonGUId = "658cd35d788af46489726e1322cc904e";

		#endregion

		#region Init
		/// <summary>
		/// Maps actions, and hides the description option if no file is assigned to the inspected item.
		/// </summary>
		/// <param name="_asset">Input asset with inpection map.</param>
		/// <param name="_item">Inspected item.</param>
		public void Init(InputActionAsset _asset, InteractableItem _item, AsyncOperationHandle<SceneInstance> _scene)
        {
            scene = _scene;
            asset = _asset;
            item = _item;

            endAction = asset.actionMaps[2].actions[0];
            infoAction = asset.actionMaps[2].actions[1];
            takeAction = asset.actionMaps[2].actions[2];

            asset.actionMaps[2].Enable();

            //Texture2D a = Addressables.LoadAssetAsync<Texture2D>(buttonGUId).Result;
            doc = GetComponent<UIDocument>();
            doc.enabled = true;
            doc.rootVisualElement.Q<Label>(TITLE).text = _item.ItemName;

            if(item.SourceObject.IsValid())
            {
                infoAction.Disable();
                doc.rootVisualElement.Q<VisualElement>(DESCRIPTION_OPTION).style.display = DisplayStyle.None;
            }
            isDescriptionOpened = false;

            transform.parent.GetChild(2).GetComponent<InspectionController>().Init(this, _asset, item);
            enabled = true;
        }
        #endregion

        void Update()
        {
            if (endAction.triggered)
                EndInteract();
            else if (infoAction.triggered)
                DescriptionToggle();
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
            StartCoroutine(EnableMovement());
        }

        /// <summary>
        /// Unloads interaction scene and resets player camera.
        /// </summary>
        /// <returns></returns>
        IEnumerator EnableMovement()
		{
			CrosshairImage.Toggle();
            yield return new();
            Camera.main.cullingMask = -1;
            gameObject.SetActive(false);
			AsyncOperationHandle<SceneInstance> sceneUnload = Addressables.UnloadSceneAsync(scene, UnloadSceneOptions.None, false);
            sceneUnload.Completed += (_) =>
            {
				asset.actionMaps[0].Enable();
				GameObject.FindFirstObjectByType<PlayerCamera>().EndIteract();
			};
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
                doc.rootVisualElement.Q<VisualElement>(HOME_OPTION).style.display = DisplayStyle.None;// "Zavřít popis";
                
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
                doc.rootVisualElement.Q<VisualElement>(HOME_OPTION).style.display = DisplayStyle.Flex;// "Zavřít popis";


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