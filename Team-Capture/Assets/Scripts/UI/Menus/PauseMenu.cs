// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine.Localization;

namespace Team_Capture.UI.Menus
{
    /// <summary>
    ///     Menu for the pause menu
    /// </summary>
    internal class PauseMenu : MenuController
    {
        public MenuPanel[] menuPanels;

        public LocalizedString menuResumeText;
        
        private Action togglePauseMenu;
        
        public void Setup(Action toggleMenu)
        {
            togglePauseMenu = toggleMenu;
            
            CreateButton(topButtonPrefab, topNavBar, menuResumeText.GetLocalizedString(), ClosePauseMenu, 52f, true);
            
            AddPanels(menuPanels);
        }

        private void ClosePauseMenu()
        {
            CloseActivePanel();
            
            togglePauseMenu.Invoke();
        }
    }
}