using Assets.Scripts.Blur;
using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ItemInteract : MonoBehaviour
{
    [SerializeField] CinemachineCamera cam;
    [SerializeField] Image backgroundImage;
    [SerializeField] Canvas canvas;
    [SerializeField] NewBlur blur;


    Vector3 prevPos;
    Quaternion prevRot;

    InputActionAsset asset;

    Transform item;

    InputAction endInteract;

    public void Init(Transform _item, InputActionAsset _asset)
    {
        item = _item;
        asset = _asset;
        prevPos = _item.localPosition;
        prevRot = _item.localRotation;
        transform.position = _item.position;

        _item.parent = transform;
        _item.localPosition = new(0, 0, 0);
        _item.localRotation = Quaternion.identity;
        _item.gameObject.layer = 6;


        Camera.main.cullingMask = 96;
        Camera.main.transform.parent = transform;
        Camera.main.transform.SetParent(null);
        canvas.worldCamera = Camera.main;

        endInteract = asset.actionMaps[2].actions[0];
        asset.actionMaps[2].Disable();

        StartCoroutine(WaitOnPostRender());
    }

    IEnumerator WaitOnPostRender()
    {
        //blur.CaptureAndBlurScreen();
        yield return new WaitForEndOfFrame();
        GameObject.Find("Background Image").GetComponent<Image>().sprite = GaussianBlur.Blur();
        cam.Priority = 3;
        StartCoroutine(WaitForEndOfBlend(true));
        //canvas.transform.GetChild(0).GetComponent<Image>().sprite = GaussianBlur.Blur();
    }

    private void Update()
    {
        if (endInteract.triggered)
            EndInteract();
    }

    void EndInteract()
    {
        asset.actionMaps[2].Disable();
        item.gameObject.layer = 0;
        item.SetParent(GameObject.Find("World").transform);

        canvas.gameObject.SetActive(false);
        Camera.main.transform.SetParent(GameObject.Find("EventSystem").transform);
        Camera.main.transform.SetParent(null);
        Camera.main.cullingMask = -1;
        cam.Priority = -1;
        
        StartCoroutine(WaitForEndOfBlend(false));
    }

    IEnumerator WaitForEndOfBlend(bool opening)
    {
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        while (brain.ActiveBlend == null)
            yield return null;
        yield return new WaitForSecondsRealtime(brain.ActiveBlend.Duration);
        if (opening)
        {
            asset.actionMaps[2].Enable();
        }
        else
        {
            asset.actionMaps[0].Enable();
            SceneManager.UnloadSceneAsync(1);
        }
    }
}