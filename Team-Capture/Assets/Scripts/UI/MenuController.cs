using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mirror;
using Team_Capture.Core;
using Team_Capture.Helper.Extensions;
using Team_Capture.Input;
using Team_Capture.Localization;
using Team_Capture.Tweens;
using Team_Capture.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI
{
	/// <summary>
	///     Controller for a menu
	/// </summary>
	internal class MenuController : MonoBehaviour
	{
		/// <summary>
		///     The parent for all main menu panels
		/// </summary>
		[Header("Base Settings")] [Tooltip("The parent for all main menu panels")]
		public Transform mainMenuPanel;

		/// <summary>
		///     The top nav bar, where the buttons will go
		/// </summary>
		[Tooltip("The top nav bar, where the buttons will go")]
		public Transform topNavBar;

		/// <summary>
		///     The bottom nav bar, where the buttons will go
		/// </summary>
		[Tooltip("The bottom nav bar, where the buttons will go")]
		public Transform bottomNavBar;

		/// <summary>
		///     Top top button prefab
		/// </summary>
		[Tooltip("Top top button prefab")] public GameObject topButtonPrefab;

		/// <summary>
		///     The bottom button prefab
		/// </summary>
		[Tooltip("The bottom button prefab")] public GameObject bottomButtonPrefab;

		/// <summary>
		///		Handles reading input
		/// </summary>
		public InputReader inputReader;

		[NonSerialized] public bool allowPanelToggling = true;

		private TweeningManager tweeningManager;
		private readonly Dictionary<MenuPanel, GameObject> activeMenuPanels = new Dictionary<MenuPanel, GameObject>();

		private void Awake()
		{
			tweeningManager = GetComponent<TweeningManager>();
			allowPanelToggling = true;
		}

		private void OnEnable()
		{
			inputReader.MenuClose += CloseActivePanel;
			inputReader.EnableMenuControllerInput();

			tweeningManager.GetTweenObject("TopNavBar").PlayAllEvents();
			tweeningManager.GetTweenObject("BottomNavBar").PlayAllEvents();
		}

		private void OnDisable()
		{
			inputReader.MenuClose -= CloseActivePanel;
			inputReader.DisableMenuControllerInput();
		}

		private void OnDestroy()
		{
			if (Game.IsGameQuitting)
				return;

			Logger.Debug("Resetting all main menu events...");

			DestroyButtons();
		}

		/// <summary>
		///		Adds a list of <see cref="MenuPanel"/>s
		/// </summary>
		/// <param name="panels"></param>
		protected void AddPanels(IEnumerable<MenuPanel> panels)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			
			foreach (MenuPanel menuPanel in panels)
			{
				//Create the panel
				GameObject panel = Instantiate(menuPanel.panelPrefab, mainMenuPanel);
				panel.name = menuPanel.name;
				panel.SetActive(false);
				activeMenuPanels.Add(menuPanel, panel);

				//If it has a PanelBase, set it cancel button's onClick event to toggle it self
				PanelBase panelBase = panel.GetComponent<PanelBase>();
				if (panelBase != null)
					panelBase.cancelButton.onClick
						.AddListener(delegate { TogglePanel(menuPanel.name); });

				//Create the button for it
				if (menuPanel.bottomNavBarButton)
					CreateButton(bottomButtonPrefab, bottomNavBar, menuPanel.menuButtonText,
						delegate { TogglePanel(menuPanel.name); });
				else
					CreateButton(topButtonPrefab, topNavBar, menuPanel.menuButtonText,
						delegate { TogglePanel(menuPanel.name); });
			}
			
			stopwatch.Stop();
			Logger.Debug("Time taken to update menu UI: {TotalMilliseconds}ms", stopwatch.ElapsedMilliseconds);
		}

		/// <summary>
		///     Toggles between panels
		/// </summary>
		/// <param name="panelName"></param>
		public void TogglePanel(string panelName)
		{
			if(!allowPanelToggling)
				return;

			KeyValuePair<MenuPanel, GameObject> panel = GetMenuPanel(panelName);
			if (panel.Key == null)
			{
				Logger.Error("No such panel with the name of {PanelName}!", name);
				return;
			}

			//There is a panel that is currently active, so close it
			if (GetActivePanel().Key != null && panel.Key != GetActivePanel().Key)
			{
				Logger.Debug($"{GetActivePanel().Key.name} is currently active, switching...");

				ClosePanel(GetActivePanel(), true);
				OpenPanel(panel, true);

				return;
			}

			if (!panel.Value.activeSelf)
				OpenPanel(panel);
			else
				ClosePanel(panel);
		}

		/// <summary>
		///     Closes the active panel
		/// </summary>
		protected void CloseActivePanel()
		{
			if(!allowPanelToggling)
				return;

			if (GetActivePanel().Key != null)
				ClosePanel(GetActivePanel());
		}

		protected void CreateButton(GameObject buttonPrefab, Transform parent, string text, UnityAction action,
			float maxSize = 52f, bool bold = false)
		{
			GameObject button = Instantiate(buttonPrefab, parent);
			TextMeshProUGUI tmpText = button.GetComponentInChildren<TextMeshProUGUI>();
			tmpText.text = text;
			tmpText.fontSizeMax = maxSize;
			if (bold)
				tmpText.fontStyle = FontStyles.Bold;
			button.GetComponent<Button>().onClick.AddListener(action);
			button.name = $"{text} Button";
			tmpText.gameObject.AddComponent<GameUITMPResolver>();
		}

		private void DestroyButtons()
		{
			activeMenuPanels.Clear();
			topNavBar.DestroyAllChildren();
			bottomNavBar.DestroyAllChildren();
		}

		#region Panel List Functions

		private KeyValuePair<MenuPanel, GameObject> GetMenuPanel(string panelName)
		{
			IEnumerable<KeyValuePair<MenuPanel, GameObject>> result = from a in activeMenuPanels
				where a.Key.name == panelName
				select a;

			return result.FirstOrDefault();
		}

		/// <summary>
		///     Returns the active panel's <see cref="MenuPanel" />
		/// </summary>
		/// <returns></returns>
		public KeyValuePair<MenuPanel, GameObject> GetActivePanel()
		{
			IEnumerable<KeyValuePair<MenuPanel, GameObject>> result = from a in activeMenuPanels
				where a.Value.activeSelf
				select a;

			return result.FirstOrDefault();
		}

		#endregion

		#region Animation Functions

		private void ActivateTopBlackBar()
		{
			tweeningManager.GetTweenObject("TopBlackBar").PlayEvent("TopBlackBarDown");
		}

		private void DeactivateTopBlackBar()
		{
			tweeningManager.GetTweenObject("TopBlackBar").PlayEvent("TopBlackBarUp");
		}

		private void ActivateBlackBackground()
		{
			//If we are in game we want the black background always active
			if (NetworkManager.singleton.isNetworkActive)
				return;

			tweeningManager.GetTweenObject("BlackBackground").PlayEvent("BackgroundFadeIn");
		}

		private void DeactivateBlackBackground()
		{
			//If we are in game we want the black background always active
			if (NetworkManager.singleton.isNetworkActive)
				return;

			tweeningManager.GetTweenObject("BlackBackground").PlayEvent("BackgroundFadeOut");
		}

		#endregion

		#region Panel Functions

		private void ClosePanel(KeyValuePair<MenuPanel, GameObject> panel, bool isSwitching = false)
		{
			Logger.Debug($"Closing {panel.Key.name}");

			if (!isSwitching)
			{
				if (panel.Key.showTopBlackBar)
					DeactivateTopBlackBar();

				if (panel.Key.darkenScreen)
					DeactivateBlackBackground();
			}

			panel.Value.SetActive(false);
		}

		private void OpenPanel(KeyValuePair<MenuPanel, GameObject> panel, bool isSwitching = false)
		{
			Logger.Debug($"Opening {panel.Key.name}");

			if (!isSwitching)
			{
				if (panel.Key.showTopBlackBar)
					ActivateTopBlackBar();

				if (panel.Key.darkenScreen)
					ActivateBlackBackground();
			}

			panel.Value.SetActive(true);
		}

		#endregion

		#region Pause Menu Specfic

		
		private void CloseActivePanelPauseMenu()
		{
			if(!allowPanelToggling)
				return;
			
			if (GetActivePanel().Key != null)
				ClosePanel(GetActivePanel());

			ClosePauseMenuAction();
		}

		public Action ClosePauseMenuAction;

		#endregion
	}
}