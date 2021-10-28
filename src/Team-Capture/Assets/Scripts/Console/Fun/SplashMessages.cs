// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Console.Fun
{
    [CreateAssetMenu(fileName = "SplashMessages", menuName = "Team-Capture/Settings/SplashMessages")]
    internal class SplashMessages : ScriptableObject
    {
        public string[] messages;
    }
}