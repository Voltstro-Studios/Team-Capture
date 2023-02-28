// Team-Capture
// Copyright (c) 2019-2023 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor
{
    [ExecuteAlways]
    [InitializeOnLoad]
    public class GeneratorCheck : MonoBehaviour
    {
        private const string GeneratorLocation = "Editor/Plugins/Team-Capture.Generator.dll";
        
        static GeneratorCheck()
        {
            string generatorLocationFull = Path.GetFullPath(Path.Combine(Application.dataPath, GeneratorLocation));
            if (!File.Exists(generatorLocationFull))
            {
                Debug.LogError("==============================");
                Debug.LogError($"The generator dll is missing from '{generatorLocationFull}'! You did not setup the project correctly!");
                Debug.LogError("See https://github.com/Voltstro-Studios/Team-Capture#pre-setup for building the generator!");
                Debug.LogError("==============================");
            }
        }
        
    }
}