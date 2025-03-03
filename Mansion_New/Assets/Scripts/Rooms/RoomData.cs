using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Rooms
{
    public class RoomData : ScriptableObject
    {
        public List<RoomData> AdjacentRooms;
        public string SceneName;

        private void OnValidate()
        {
            int index;
            if((index = AdjacentRooms.FindIndex(q=> q.SceneName == SceneName)) == 0)
                return;
            else if(index > 0)
                AdjacentRooms.RemoveAt(index);

            AdjacentRooms.Insert(0, this);
            EditorUtility.SetDirty(this);
        }
    }
}
