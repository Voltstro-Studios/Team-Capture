using System;

namespace Team_Capture.UI.Menus
{
    /// <summary>
    ///     Menu for the pause menu
    /// </summary>
    internal class PauseMenu : MenuController
    {
        public MenuPanel[] menuPanels;
        
        private Action togglePauseMenu;
        
        public void Setup(Action toggleMenu)
        {
            togglePauseMenu = toggleMenu;
            
            CreateButton(topButtonPrefab, topNavBar, "Menu_Resume", ClosePauseMenu, 52f, true);
            
            AddPanels(menuPanels);
        }

        private void ClosePauseMenu()
        {
            CloseActivePanel();
            
            togglePauseMenu.Invoke();
        }
    }
}