using Assets.Scripts.Interactable_Items.Rooms;
using Rooms;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.Editor.Levels.Entrances
{

    public partial class EntranceEditor : Foldout
    {
        DropdownField connection;
        Vector3Field position;
        Vector3Field size;
        
        int index;
        Transform entrance;

        public EntranceEditor()
        {
            Add(connection = new("Connection:"));
            /*Add(position = new("Position:"));
            Add(size = new("Size:"));*/
            RegisterCallback<FocusInEvent>((ev) => { Selection.activeGameObject = entrance.gameObject; });
            RegisterCallback<PointerDownEvent>((ev) => { Selection.activeGameObject = entrance.gameObject; });
        }

        public void Bind(int i, Room room, List<string> choices)
        {
            choices.Remove(room.name);
            text = choices[i];
            index = i;
            entrance = room.Entrances.GetChild(index);

            SerializedObject r = new(room);
            connection.Unbind();
            connection.choices = choices;
            connection.BindProperty(
                r.FindProperty(nameof(Room.AdjacentRooms))
                .GetArrayElementAtIndex(i));


            /*SerializedObject obj = new(entrance);
            position.Unbind();
            position.BindProperty(obj.FindProperty("m_LocalPosition"));


            SerializedObject collider = new(entrance.GetComponent<BoxCollider>());
            size.Unbind();
            size.BindProperty(collider.FindProperty("m_Size"));*/
        }
    }
}
