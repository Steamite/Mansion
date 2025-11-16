using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "PDFData", menuName = "Item Inspection/PDFData")]
public class PDFData : ScriptableObject
{
    /// <summary>Path to streaming assets.</summary>
    public string pdf;
    /// <summary>Links to .jpgs.</summary>
    public List<AssetReference> images;
}
