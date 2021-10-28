// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.UI.Panels
{
    [CreateAssetMenu(fileName = "QuitMessages", menuName = "Team-Capture/Settings/QuitMessages")]
    internal class QuitMessages : ScriptableObject
    {
        public string[] quitMessages;
    }
}