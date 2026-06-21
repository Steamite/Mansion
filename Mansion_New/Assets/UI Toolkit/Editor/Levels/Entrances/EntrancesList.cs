using Assets.Scripts.Interactable_Items.Rooms;
using Assets.UI_Toolkit.Editor.Levels.Entrances;
using Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.Editor.Levels.Items
{
    public partial class EntrancesList : ListView
    {
        LevelData data;
        List<string> choices;
        Room room;

        public EntrancesList()
        {
            this.InitStyles("Entrances");
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            makeItem = () => new EntranceEditor();
            bindItem = (el, i) =>
            {
                EntranceEditor entrance = el as EntranceEditor;
                entrance.Bind(
                    i, 
                    room, 
                    data.scenes.ToList()
                    );
            };

            onAdd = (_) =>
            {
                GameObject entrance = new("Entrance");
                entrance.transform.parent = room.Entrances;
                entrance.transform.localPosition = new();
                BoxCollider collider = entrance.AddComponent<BoxCollider>();
                collider.size = new(0.1f, 0.1f, 1);
                collider.isTrigger = true;
                room.AdjacentRooms.Add(null);

                EditorUtility.SetDirty(room);
                itemsSource = room.Entrances.GetComponentsInChildren<BoxCollider>();
                RefreshItems();
                selectedIndex = itemsSource.Count - 1;
            };

            onRemove = (_) =>
            {
                int x = selectedIndex;
                room.AdjacentRooms.RemoveAt(x);
                GameObject.DestroyImmediate(room.Entrances.GetChild(x).gameObject);
                itemsSource = room.Entrances.GetComponentsInChildren<BoxCollider>();
                RefreshItems();
            };
        }

        public void Bind(Room _room, LevelData levelData)
        {
            room = _room;
            data = levelData;
            choices = levelData.scenes;
            itemsSource = room.Entrances.GetComponentsInChildren<BoxCollider>();
            RefreshItems();
        }
    }
}
