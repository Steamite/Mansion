using Items;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class InspectionController : MonoBehaviour
{

    InputAction zoomAction;

    InputAction startRotateAction;

    InputAction stopRotateAction;

    InputAction startDragAction;

    InputAction stopDragAction;

    InputAction dragAction;

    InputAction resetViewAction;

    IInspectMenu menu;

    bool canDrag;
    CinemachinePositionComposer positionComposer;
    CinemachinePanTilt panTilt;
    CinemachineInputAxisController axisController;

    InteractableItem item;

    public void Init(IInspectMenu _menu, InputActionAsset asset, InteractableItem _item)
    {
        menu = _menu;
        item = _item;

        zoomAction = asset.actionMaps[2].actions[3];
        startRotateAction = asset.actionMaps[2].actions[4];
        stopRotateAction = asset.actionMaps[2].actions[5];
        dragAction = asset.actionMaps[2].actions[6];
        startDragAction = asset.actionMaps[2].actions[7];
        stopDragAction = asset.actionMaps[2].actions[8];
        resetViewAction = asset.actionMaps[2].actions[9];


        enabled = true;

        axisController = GetComponent<CinemachineInputAxisController>();
        panTilt = GetComponent<CinemachinePanTilt>();
        TogglePanTilt(false);
        positionComposer = GetComponent<CinemachinePositionComposer>();
    }

    // Update is called once per frame
    void Update()
    {

        if (resetViewAction.triggered)
            ResetView();
        else
        {
            if (zoomAction.triggered && menu.isDescrOpen == false)
                Zoom();
            if (startRotateAction.triggered)
                TogglePanTilt(true);
            else if (stopRotateAction.triggered)
                TogglePanTilt(false);

            if (startDragAction.triggered)
                ToggleDrag(true);
            else if (stopDragAction.triggered)
                ToggleDrag(false);
            if (canDrag && menu.isDescrOpen == false)
                Drag();
        }
    }

    #region Control Actions
    void Zoom()
    {
        Vector2 f = zoomAction.ReadValue<Vector2>();
        positionComposer.CameraDistance = Mathf.Clamp(positionComposer.CameraDistance - f.y * 0.1f, item.RadiusRange.x, item.RadiusRange.y);
        Debug.Log(f);
    }

    void TogglePanTilt(bool newState)
    {
        axisController.Controllers[0].Enabled = newState;
        axisController.Controllers[1].Enabled = newState;
    }

    void ToggleDrag(bool newState)
    {
        canDrag = newState;
    }

    void Drag()
    {
        Vector2 drag = dragAction.ReadValue<Vector2>();
        positionComposer.Composition.ScreenPosition.x = Mathf.Clamp(positionComposer.Composition.ScreenPosition.x + drag.x, -0.5f * 1 / positionComposer.CameraDistance, 0.5f * 1 / positionComposer.CameraDistance);
        positionComposer.Composition.ScreenPosition.y = Mathf.Clamp(positionComposer.Composition.ScreenPosition.y + drag.y, -0.5f * 1 / positionComposer.CameraDistance, 0.5f * 1 / positionComposer.CameraDistance);
    }

    void ResetView()
    {
        positionComposer.Composition.ScreenPosition.x = 0;
        positionComposer.Composition.ScreenPosition.y = 0;
        panTilt.PanAxis.Value = item.startRotation.x;
        panTilt.TiltAxis.Value = item.startRotation.y;
    }
    #endregion
}
