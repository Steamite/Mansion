using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PDFData", menuName = "Item Inspection/PDFData")]
public class PDFData : ScriptableObject
{

    /// <summary>Path to streaming assets.</summary>
    [FormerlySerializedAs("pdf")]
    public string pdfName;
    /// <summary>Links to .jpgs.</summary>
    public List<AssetReference> images;
}
