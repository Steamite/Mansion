using Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.UI_Toolkit.Editor.Levels.Items
{

    [UxmlElement]
    public partial class ItemList : ListView
    {
        public InteractableItem SelectedItem =>
            selectedIndex > -1 && selectedIndex < itemsInRoom.Count
                ? itemsInRoom[selectedIndex]
                : null;

        List<InteractableItem> itemsInRoom;
        
        public ItemList() { }
        public ItemList(RoomEditor roomEditor)
        {
            this.InitStyles("Interactables");
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            makeItem = () => new ItemEditor(roomEditor);
            bindItem = (e, i) => (e as ItemEditor).Bind(itemsInRoom[i]);
            onAdd = (_) =>
            {
                GameObject item = new(
                    "",
                    typeof(MeshFilter), typeof(MeshRenderer),
                    typeof(TextItem), typeof(BoxCollider));
                item.layer = LayerMask.NameToLayer("Interactables");
                item.transform.parent = roomEditor.SelectedRoom.Interactables;
                item.transform.SetLocalPositionAndRotation(
                    new(0, 0, 0),
                    Quaternion.Euler(0, 0, 0));

                EditorSceneManager.SaveScene(
                    EditorSceneManager.GetSceneByPath(roomEditor.RoomScene));
                RefreshItems();
            };

            onRemove = (_) =>
            {
                int i = selectedIndex;
                if (i < 0 || i > itemsInRoom.Count)
                    return;

                roomEditor.SelectedRoom.Interactables.GetChild(i).gameObject.SetActive(false);
                EditorSceneManager.SaveScene(
                    EditorSceneManager.GetSceneByPath(roomEditor.RoomScene));
                RefreshItems();
            };
            selectionChanged += (_) =>
            {
                itemsInRoom[selectedIndex].Zoom();
            };
        }

        public void Load(List<InteractableItem> _interactableItems)
        {
            itemsInRoom = _interactableItems;
            itemsSource = itemsInRoom;
            RefreshItems();
        }
    }
}
