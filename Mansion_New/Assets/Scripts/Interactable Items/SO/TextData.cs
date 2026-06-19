using Assets.Scripts.Interactable_Items.SO;
using UnityEngine;

[CreateAssetMenu(fileName = "TextData", menuName = "Item Inspection/TextData")]
public class TextData : ItemData
{
    [HideInInspector] public string content;
}