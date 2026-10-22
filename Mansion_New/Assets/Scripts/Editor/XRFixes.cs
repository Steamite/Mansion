using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Editor
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.XR.Management;

    [InitializeOnLoad]
    public static class XRSubsystemReset
    {
        static XRSubsystemReset()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            Debug.Log(state);
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                // Stop and de-initialize active XR loaders to clear subsystem descriptors before the next Play entry
                /*if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
                {
                    var manager = XRGeneralSettings.Instance.Manager;
                    if (manager.isInitializationComplete)
                    {
                        manager.StopSubsystems();
                        manager.DeinitializeLoader();
                    }
                }*/
                var manager = XRGeneralSettings.Instance?.Manager;
                if (manager != null)
                {
                    // Stop subsystems if active loader is non-null, regardless of initialization state flag
                    if (manager.activeLoader != null)
                    {
                        manager.StopSubsystems();
                        manager.DeinitializeLoader();
                    }
                }

                var activeSubsystems = new List<ISubsystem>();
                SubsystemManager.GetSubsystems(activeSubsystems);
                foreach (var subsystem in activeSubsystems)
                {
                    if (subsystem.running)
                    {
                        subsystem.Stop();
                    }
                    subsystem.Destroy();
                }
            }
        }
    }
#endif
}
