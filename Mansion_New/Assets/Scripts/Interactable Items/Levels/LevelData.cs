using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Interactable_Items.Rooms
{
    public class LevelData : ScriptableObject
    {
        public string WorldName = "";
        public bool active = true;
        public List<string> scenes = new();
        public Vector3 spawn;
        public int initScene;
    }
}
