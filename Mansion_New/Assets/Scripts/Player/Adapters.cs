using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UIElements;

public interface ICrosshairImage
{
    void EndHold();
    void StartHold();
    void Exit();
    void Enter();
    void Toggle(bool show);
}

public interface IInspectMenu
{
    bool isDescrOpen { get; }
}

public interface IInspectionInit
{
    public void Init(Transform _item, InputActionAsset _asset, AsyncOperationHandle<SceneInstance> _scene) { }
}