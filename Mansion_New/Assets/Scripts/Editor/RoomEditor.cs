using System.Collections.Generic;
using System.ComponentModel;
using Rooms;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    Room room;
    SerializedObject GetTarget;
    SerializedProperty ThisList;
    int ListSize;

    int selectedIndex;
    void OnEnable(){
        room = (Room)target;
        GetTarget = new SerializedObject(room);
        ThisList = GetTarget.FindProperty(nameof(Room.AdjacentRooms)); // Find the List in our script and create a refrence of it
    }

    public override void OnInspectorGUI(){    
        EditorGUILayout.LabelField("Define the list size with a number");
        ListSize = ThisList.arraySize;
        ListSize = EditorGUILayout.IntField ("List Size", ListSize);
    
        if(ListSize != ThisList.arraySize){
            while(ListSize > ThisList.arraySize){
                ThisList.InsertArrayElementAtIndex(ThisList.arraySize);
            }
            while(ListSize < ThisList.arraySize){
                ThisList.DeleteArrayElementAtIndex(ThisList.arraySize - 1);
            }
        }
    
        EditorGUILayout.Space ();
    
        //Or add a new item to the List<> with a butto
        EditorGUILayout.LabelField("Add a new room connection:");
    
        if(GUILayout.Button("Add New")){
            room.AdjacentRooms.Add("");
            EditorUtility.SetDirty(target);
        }
    
        EditorGUILayout.Space ();

        string scene;
        for(int i = 0; i < room.AdjacentRooms.Count; i++){
            scene = ((SceneAsset) EditorGUILayout.ObjectField(
                AssetDatabase.LoadAssetAtPath<SceneAsset>($"Assets/Scenes/Room Scenes/{room.AdjacentRooms[i]}.unity"), typeof(SceneAsset), false))?.name;
            if(scene != room.AdjacentRooms[i]){
                room.AdjacentRooms[i] = scene;
                EditorUtility.SetDirty(target);
            }
                
        }
        if(ListSize > 0){
            EditorGUILayout.Space();
            GUI.enabled = room.AdjacentRooms.FindIndex(q => q == "" || q == null) > -1;
            if(GUILayout.Button("Remove Empty")){
                room.AdjacentRooms.RemoveAll(q=> q == "" || q == null);
                EditorUtility.SetDirty(target);
            }
        }
    }
}
