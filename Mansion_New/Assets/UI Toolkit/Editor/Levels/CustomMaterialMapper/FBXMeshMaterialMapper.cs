using ImageMagick.Drawing;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FBXMeshMaterialMapper
{
    public struct MeshObjectData
    {
        public Material[] materials;
        public GameObject meshObject;

        public MeshObjectData(Material[] mat, GameObject mO)
        {
            materials = mat;
            meshObject = mO;
        }
    }

    public static MeshObjectData GetMeshMaterials(Mesh searchMesh)
    {
        string path = AssetDatabase.GetAssetPath(searchMesh);
        if (string.IsNullOrEmpty(path))
            return new(new Material[0], null);

        GameObject fbxRoot = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;

        foreach (var renderer in fbxRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh mesh = renderer.sharedMesh;
            if (mesh == null || !mesh.Equals(searchMesh))
                continue;

            return new(renderer.GetComponent<MeshRenderer>().sharedMaterials, renderer.gameObject);
        }
        return new(new Material[0], null);
    }
}