using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Interactable_Items.Rooms
{
    public class LevelData : ScriptableObject
    {
        public const string LEVEL_DATA_PATH = "Assets/Levels/";
        public const string LEVEL_SCENE_PATH = "Assets/Scenes/Levels/";
        public string SceneFolderPath => LEVEL_SCENE_PATH + WorldName+"/";
        public string LightPath => SceneFolderPath + "Lightning.unity";
        public string GetRoomPath(int i) => SceneFolderPath + $"{scenes[i]}.unity";
        public string GetRoomPath(string scene) => SceneFolderPath + $"{scene}.unity";

        public string WorldName = "";
        public bool active = true;
        public List<string> scenes = new();
        public Vector3 spawn;
        public int initScene;
    }
}
