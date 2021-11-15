using System.Collections;
using System.Collections.Generic;
using Team_Capture.Settings;
using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor
{
    [InitializeOnLoad]
    public class LoadSettings
    {
        static LoadSettings()
        {
            GameSettings.Load();
        }
    }
}
