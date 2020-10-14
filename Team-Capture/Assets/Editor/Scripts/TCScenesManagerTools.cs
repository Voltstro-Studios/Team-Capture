#if UNITY_EDITOR

using System.IO;
using System.Linq;
using SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Voltstro.UnityBuilder.Build;

namespace Editor.Scripts
{
    public static class TCScenesManagerTools
    {
        [MenuItem("Team Capture/List All Scenes")]
        private static void ListAllTCScenes()
        {
            Debug.Log($"{nameof(TCScene)}s found:");
            foreach (TCScene tcScene in TCScenesManager.GetAllTCScenesInfo())
                Debug.Log($"{tcScene.scene} ({tcScene.displayName})");
        }

        [MenuItem("Team Capture/List Enabled Scenes")]
        private static void ListAllEnabledScenes()
        {
            Debug.Log($"Enabled {nameof(TCScene)}s found:");
            foreach (TCScene tcScene in TCScenesManager.GetAllEnabledTCScenesInfo())
                Debug.Log($"{tcScene.scene} ({tcScene.displayName})");
        }

        [MenuItem("Team Capture/Run Scene")]
        private static void RunActiveScene()
        {
            //Get the active scene's TCScene file
	        string activeSceneName = SceneManager.GetActiveScene().name;
	        TCScene scene = TCScenesManager.GetAllEnabledTCScenesInfo().FirstOrDefault(s => s.name == activeSceneName);

            //It don't exists!
	        if (scene == null)
	        {
                Debug.LogError("There is no associated TCScene file with this scene or it is not enabled!");
                return;
	        }

            //Its an offline scene
	        if (!scene.isOnlineScene)
	        {
                Debug.LogError("This scene is not an online scene!");
                return;
	        }

            //Ok, now we need to see if the build has this version of the scene
            string buildDir = $"{GameBuilder.GetBuildDirectory()}Team-Capture-Quick/Team-Capture_Data/";
            if (Directory.Exists(buildDir))
            {
	            string sceneFilePath = $"{buildDir}level{SceneManager.GetSceneByPath(scene.scene).buildIndex}";
	            if (File.Exists(sceneFilePath))
	            {
		            if (File.GetLastWriteTime(sceneFilePath) == File.GetLastWriteTime(scene.scene))
		            {
                        Debug.Log("Scenes are in date");
                        return;
		            }

                    Debug.Log("Out of sync scenes!");
	            }
            }
        }
    }
}

#endif