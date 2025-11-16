using Rooms;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    Room room;

    public override VisualElement CreateInspectorGUI()
    {
        room = (Room)target;
        VisualElement element = new();

        ListView listView = new()
        {
            showAddRemoveFooter = true,
            showFoldoutHeader = true,
            headerTitle = "Adjacent Rooms"
        };
        listView.makeItem = () => new ObjectField()
        {
            objectType = typeof(SceneAsset),
            allowSceneObjects = false,
            style =
            {
                paddingLeft = new Length(2, LengthUnit.Percent),
                paddingRight = new Length(10, LengthUnit.Percent),

            }
        };
        listView.bindItem = (el, i) =>
        {
            ObjectField field = el as ObjectField;
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>($"Assets/Scenes/Room Scenes/{room.AdjacentRooms[i]}.unity");
            field.value = sceneAsset;
            var x = i;
            field.RegisterValueChangedCallback((evt) => OnSceneChange(evt, x));
        };

        listView.onAdd = (list) =>
        {
            room.AdjacentRooms.Add(new(""));
            EditorUtility.SetDirty(room);
            listView.itemsSource = room.AdjacentRooms;
        };

        listView.onRemove = (list) =>
        {
            room.AdjacentRooms.RemoveAt(list.selectedIndex);
            EditorUtility.SetDirty(room);
            listView.itemsSource = room.AdjacentRooms;
        };

        element.Add(listView);
        listView.itemsSource = room.AdjacentRooms;

        ObjectField field = new("Entrances") { objectType = typeof(GameObject) };
        field.value = room.entrances;
        field.RegisterValueChangedCallback((evt) => OnEntranceChange(evt.newValue));
        element.Add(field);

        return element;
    }

    void OnSceneChange(ChangeEvent<Object> evt, int i)
    {
        Object obj = evt.newValue;
        if (obj.name == room.gameObject.scene.name)
        {
            (evt.target as ObjectField).SetValueWithoutNotify(evt.previousValue);
            Debug.LogWarning("cannot add same scene");
            return;
        }

        room.AdjacentRooms[i] = obj.name;
        EditorUtility.SetDirty(room);
    }

    void OnEntranceChange(Object obj)
    {
        room.entrances = (obj as GameObject).transform;
        EditorUtility.SetDirty(room);
    }
}
