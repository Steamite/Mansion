using Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Importer
{
    [CreateAssetMenu(fileName ="ImporterBackup", menuName ="Item Inspection/Bck", order=1)]
    class ImporterBackup : ScriptableObject
    {
        [Serializable]
        struct Link
        {
            [SerializeField] public SceneAsset scene;
            [SerializeField] public string content;
            public Link(SceneAsset _scene, string _content)
			{
                scene = _scene;
                content = _content;
			}
		}

        static string[] paths;
		[SerializeField] List<Link> linkedObjects;

        public void AddData(Scene asset, InteractableItem item)
        {
            Link data = new(AssetDatabase.LoadAssetAtPath<SceneAsset>(asset.path), 
                item.SourceObjectName);
            linkedObjects.Add(data);
            EditorUtility.SetDirty(this);
        }

        public void RemoveData(string content)
        {
			linkedObjects.RemoveAll(q => q.content == content);
        }

        public void LoadData(List<string> contentItems, bool text)
        {
            List<Link> links = linkedObjects.Where(q => contentItems.Contains(q.content)).OrderBy(q => q.scene.name).ToList();
            List<Scene> scenes = new();
            for (int i = EditorSceneManager.sceneCount -1; i >= 0; i--)
            {
                scenes.Add(EditorSceneManager.GetSceneAt(i));
            }
			paths = scenes.Select(q => q.path).ToArray();

			string sceneName = "";
            List<InteractableItem> sceneItems = null; 
            for (int i = 0; i < links.Count; i++)
            {
                if (sceneName != links[i].scene.name)
                {
                    sceneName = links[i].scene.name;
                    EditorSceneManager.SaveOpenScenes();
					EditorSceneManager.OpenScene("Assets/Scenes/Rooms/" + sceneName + ".unity");
                    sceneItems = GameObject.FindObjectsByType<InteractableItem>(
                        FindObjectsInactive.Include, 
                        FindObjectsSortMode.InstanceID).ToList();
				}
                int j = sceneItems.FindIndex(q => q.SourceObjectName == links[i].content);
                if (j > -1)
                {
                    if (text)
                    {
					    sceneItems[j].SetSource(new AssetReference(AssetDatabase.AssetPathToGUID($"Assets/ItemData/Text/{sceneItems[j].SourceObjectName}.asset")), sceneItems[j].SourceObjectName);// linkAction(sceneItems[j].SourceObjectName);
                    }
                    else
                    {
						sceneItems[j].SetSource(new AssetReference(AssetDatabase.AssetPathToGUID($"Assets/ItemData/PDF/{sceneItems[j].SourceObjectName}.asset")), sceneItems[j].SourceObjectName);
					}
                    EditorUtility.SetDirty(sceneItems[i]);
				}
				else
                    Debug.LogError($"You removed Item that uses: {links[i].content}");
			}

			EditorSceneManager.SaveOpenScenes();

			EditorSceneManager.OpenScene(paths[0]);
			for (int i = 1; i < paths.Length; i++)
			{
                EditorSceneManager.OpenScene(paths[i], OpenSceneMode.Additive);
			}
		}
    }
}
