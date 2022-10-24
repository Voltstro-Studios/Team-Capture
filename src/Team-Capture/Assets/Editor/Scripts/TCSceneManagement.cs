using System.Collections.Generic;
using NetFabric.Hyperlinq;
using Team_Capture.AddressablesAddons;
using Team_Capture.Collections;
using Team_Capture.Pickups;
using Team_Capture.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Team_Capture.Editor
{
    [InitializeOnLoad]
    public class TCSceneManagement
    {
        static TCSceneManagement()
        {
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        [MenuItem("Team-Capture/Scenes/Update Scene Pooled Objects")]
        private static void UpdateScenePooledObjects()
        {
            UpdateScenePooledObjects(TCScenesManager.GetActiveScene());
        }

        private static void OnSceneSaved(Scene scene)
        {
            UpdateScenePooledObjects(TCScenesManager.FindSceneInfo(scene.name));
        }

        private static void UpdateScenePooledObjects(TCScene tcScene)
        {
            if (tcScene == null)
            {
                Debug.LogError("TCScene not found!");
                return;
            }
            
            WeaponPickup[] pickups = Object.FindObjectsOfType<WeaponPickup>();
            GenericDictionary<CachedAddressable<GameObject>, bool> objectsToCache = new();
            foreach (WeaponPickup weaponPickup in pickups)
            {
                KeyValuePair<CachedAddressable<GameObject>, bool>[] objects = weaponPickup.weapon.GetObjectsNeededToBePooled();
                foreach (KeyValuePair<CachedAddressable<GameObject>,bool> addressableObject in objects)
                {
                    if(addressableObject.Key == null)
                        continue;

                    //Yes, I know, Dictionary has 'Contains' method, but they don't seem to be working
                    if(!objectsToCache.AsValueEnumerable().Any(x => x.Key.Equals(addressableObject.Key)))
                        objectsToCache.Add(addressableObject.Key, addressableObject.Value);
                }
            }

            Debug.Log($"Added {objectsToCache.Count} of pooled objects to the scene.");
            tcScene.pooledObjects = objectsToCache;
            EditorUtility.SetDirty(tcScene);
            
            Debug.Log($"Scene saved: {tcScene.scene}");
        }
    }
}
