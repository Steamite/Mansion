using UnityEngine;

[CreateAssetMenu(fileName = "TextData", menuName = "Item Inspection/TextData")]
public class TextData : ScriptableObject
{
    [HideInInspector] public string content;
}