using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panels;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Black Background")] public Animator blackBackgroundAnimator;

    public string blackBackgroundCloseTriggerName = "Exit";

    public float blackBackgroundWaitTime = 0.2f;
    public Transform mainMenuPanel;

    public List<MainMenuPanel> menuPanels = new List<MainMenuPanel>();

    [Header("Top Black Bar")] public Animator topBlackBarAnimator;

    public string topBlackBarCloseTriggerName = "Exit";

    public float topBlackBarWaitTime = 0.2f;

    public void Start()
    {
        topBlackBarAnimator.gameObject.SetActive(false);

        //Pre create all panels
        foreach (MainMenuPanel menuPanel in menuPanels)
        {
            GameObject panel = Instantiate(menuPanel.panelPrefab, mainMenuPanel);
            panel.name = menuPanel.panelName;
            panel.SetActive(false);
            menuPanel.panelObject = panel;

            //If the panel is a quit panel, then set its no button to toggle the panel
            if (panel.GetComponent<QuitPanel>() != null)
                panel.GetComponent<QuitPanel>().noBtn.onClick
                    .AddListener(delegate { TogglePanel(menuPanel.panelName); });

            //Basically, the same thing above
            if (panel.GetComponent<CreateGamePanel>() != null)
                panel.GetComponent<CreateGamePanel>().cancelButton.onClick.AddListener(delegate
                {
                    TogglePanel(menuPanel.panelName);
                });
        }
    }

    /// <summary>
    ///     Toggles between panels
    /// </summary>
    /// <param name="panelName"></param>
    public void TogglePanel(string panelName)
    {
        MainMenuPanel panel = GetMenuPanel(panelName);

        //There is a panel that is currently active, so close it
        if (GetActivePanel() != null && panel != GetActivePanel())
        {
            Debug.Log($"{GetActivePanel().panelName} is currently active, switching...");

            ClosePanel(GetActivePanel(), true);
        }

        if (!panel.isOpen)
            OpenPanel(panel);
        else
            ClosePanel(panel);
    }

    public void CloseActivePanel()
    {
        if (GetActivePanel() != null)
            ClosePanel(GetActivePanel());
    }

    [Serializable]
    public class MainMenuPanel
    {
        [HideInInspector] public bool isOpen;
        public TCMainMenuEvent menuEvent;
        public string panelName;

        [HideInInspector] public GameObject panelObject;
        public GameObject panelPrefab;
    }

    #region Panel List Functions

    private MainMenuPanel GetMenuPanel(string panelName)
    {
        IEnumerable<MainMenuPanel> result = from a in menuPanels
            where a.panelName == panelName
            select a;

        return result.FirstOrDefault();
    }

    private MainMenuPanel GetActivePanel()
    {
        IEnumerable<MainMenuPanel> result = from a in menuPanels
            where a.isOpen
            select a;

        return result.FirstOrDefault();
    }

    #endregion

    #region Animation Functions

    private void ActivateTopBlackBar()
    {
        topBlackBarAnimator.gameObject.SetActive(true);
    }

    private IEnumerator DeactivateTopBlackBar()
    {
        topBlackBarAnimator.SetTrigger(topBlackBarCloseTriggerName);
        yield return new WaitForSeconds(topBlackBarWaitTime);
        topBlackBarAnimator.gameObject.SetActive(false);
    }

    private void ActivateBlackBackground()
    {
        blackBackgroundAnimator.gameObject.SetActive(true);
    }

    private IEnumerator DeactivateBlackBackground()
    {
        blackBackgroundAnimator.SetTrigger(blackBackgroundCloseTriggerName);
        yield return new WaitForSeconds(blackBackgroundWaitTime);
        blackBackgroundAnimator.gameObject.SetActive(false);
    }

    #endregion

    #region Panel Functions

    private void ClosePanel(MainMenuPanel panel, bool isSwitching = false)
    {
        Debug.Log($"Closing {panel.panelName}");

        if (!isSwitching)
        {
            if (panel.menuEvent.showTopBlackBar)
                StartCoroutine(DeactivateTopBlackBar());

            if (panel.menuEvent.darkenScreen)
                StartCoroutine(DeactivateBlackBackground());
        }

        panel.panelObject.SetActive(false);
        panel.isOpen = false;
    }

    private void OpenPanel(MainMenuPanel panel)
    {
        Debug.Log($"Opening {panel.panelName}");

        if (panel.menuEvent.showTopBlackBar)
            ActivateTopBlackBar();

        if (panel.menuEvent.darkenScreen)
            ActivateBlackBackground();

        panel.panelObject.SetActive(true);
        panel.isOpen = true;
    }

    #endregion
}