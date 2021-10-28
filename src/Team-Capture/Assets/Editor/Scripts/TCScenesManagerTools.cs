// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor
{
    public static class TCScenesManagerTools
    {
        [MenuItem("Team-Capture/Scenes/List All Scenes")]
        private static void ListAllTCScenes()
        {
            Debug.Log($"{nameof(TCScene)}s found:");
            foreach (TCScene tcScene in TCScenesManager.GetAllScenes())
                Debug.Log($"{tcScene.scene} ({tcScene.SceneFileName})");
        }
        
        [MenuItem("Team-Capture/Scenes/List Online Scenes")]
        private static void ListAllEnabledScenes()
        {
            Debug.Log($"Online {nameof(TCScene)}s found:");
            foreach (TCScene tcScene in TCScenesManager.GetAllOnlineScenes())
                Debug.Log($"{tcScene.scene} ({tcScene.SceneFileName})");
        }
    }
}