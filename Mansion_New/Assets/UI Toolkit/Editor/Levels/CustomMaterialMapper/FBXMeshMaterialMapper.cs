using ImageMagick.Drawing;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FBXMeshMaterialMapper
{
    public static Material[] GetMeshMaterials(Mesh searchMesh)
    {
        string path = AssetDatabase.GetAssetPath(searchMesh);
        if (string.IsNullOrEmpty(path))
            return new Material[0];

        GameObject fbxRoot = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;

        foreach (var renderer in fbxRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh mesh = renderer.sharedMesh;
            if (mesh == null || !mesh.Equals(searchMesh))
                continue;

            return renderer.GetComponent<MeshRenderer>().sharedMaterials;
        }
        return new Material[0];
    }
}