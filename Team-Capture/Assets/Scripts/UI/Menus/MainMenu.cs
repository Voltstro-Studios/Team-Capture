// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine.Localization;

namespace Team_Capture.UI.Menus
{
    /// <summary>
    ///     Menu for the main menu
    /// </summary>
    internal class MainMenu : MenuController
    {
        public LocalizedString menuHomeText;

        public MenuPanel[] menuPanels;

        private void Start()
        {
            CreateButton(topButtonPrefab, topNavBar, menuHomeText.GetLocalizedString(), CloseActivePanel, 52f, true);

            AddPanels(menuPanels);
        }
    }
}