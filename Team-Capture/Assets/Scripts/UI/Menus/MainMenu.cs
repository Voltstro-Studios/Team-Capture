namespace Team_Capture.UI.Menus
{
    /// <summary>
    ///     Menu for the main menu
    /// </summary>
    internal class MainMenu : MenuController
    {
        public MenuPanel[] menuPanels;
        
        private void Start()
        {
            CreateButton(topButtonPrefab, topNavBar, "Menu_Home", CloseActivePanel, 52f, true);
            
            AddPanels(menuPanels);
        }
    }
}