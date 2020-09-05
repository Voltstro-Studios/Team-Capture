﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mirror;
using TMPro;
using Tweens;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Logger = Core.Logging.Logger;

namespace UI
{
	/// <summary>
	/// Controller for a main menu
	/// </summary>
	public class MainMenuController : MonoBehaviour
	{
		/// <summary>
		/// The parent for all main menu panels
		/// </summary>
		[Header("Base Settings")]
		[Tooltip("The parent for all main menu panels")]
		public Transform mainMenuPanel;

		/// <summary>
		/// The top nav bar, where the buttons will go
		/// </summary>
		[Tooltip("The top nav bar, where the buttons will go")]
		public Transform topNavBar;

		/// <summary>
		/// The bottom nav bar, where the buttons will go
		/// </summary>
		[Tooltip("The bottom nav bar, where the buttons will go")]
		public Transform bottomNavBar;

		/// <summary>
		/// Top top button prefab
		/// </summary>
		[Tooltip("Top top button prefab")]
		public GameObject topButtonPrefab;

		/// <summary>
		/// The bottom button prefab
		/// </summary>
		[Tooltip("The bottom button prefab")]
		public GameObject bottomButtonPrefab;

		/// <summary>
		/// All the main menu panels that this main menu will have
		/// </summary>
		[Tooltip("All the main menu panels that this main menu will have")]
		public MainMenuPanel[] menuPanels;

		/// <summary>
		/// The key to use to close the current panel
		/// </summary>
		[Tooltip("The key to use to close the current panel")]
		public KeyCode closeKey = KeyCode.Escape;

		/// <summary>
		/// The <see cref="Animator"/> for darkening the background
		/// </summary>
		[Header("Black Background")]
		[Tooltip("The Animator for darkening the background")]
		public Animator blackBackgroundAnimator;

		/// <summary>
		/// The trigger to use when to tell the background to close (more like go away)
		/// </summary>
		[Tooltip("The trigger to use when to tell the background to close (more like go away)")]
		public string blackBackgroundCloseTriggerName = "Exit";

		/// <summary>
		/// The delay before the <see cref="blackBackgroundAnimator"/>'s <see cref="GameObject"/> is disabled
		/// </summary>
		[Tooltip("The delay before the blackBackgroundAnimator's GameObject is disabled")]
		public float blackBackgroundWaitTime = 0.2f;

		/// <summary>
		/// The <see cref="Animator"/> for the top black bar
		/// </summary>
		[Header("Top Black Bar")]
		[Tooltip("The Animator for the top black bar")]
		public Animator topBlackBarAnimator;

		/// <summary>
		/// The trigger to use when to tell the top black bar to close
		/// </summary>
		[Tooltip("The trigger to use when to tell the top black bar to close")]
		public string topBlackBarCloseTriggerName = "Exit";

		/// <summary>
		/// The delay before the <see cref="topBlackBarAnimator"/>'s <see cref="GameObject"/> is disabled
		/// </summary>
		[Tooltip("The delay before the topBlackBarAnimator's GameObject is disabled")]
		public float topBlackBarWaitTime = 0.2f;

		private TweeningManager tweeningManager;

		private void Start()
		{
			tweeningManager = GetComponent<TweeningManager>();
			topBlackBarAnimator.gameObject.SetActive(false);

			Stopwatch stopwatch = Stopwatch.StartNew();

			//Create home/return button
			string backButtonText = "Home";
			if(NetworkManager.singleton != null)
				if (NetworkManager.singleton.isNetworkActive)
					backButtonText = "Return";
			CreateButton(topButtonPrefab, topNavBar, backButtonText, CloseActivePanel, 69f);

			//Pre-create all panels and button
			foreach (MainMenuPanel menuPanel in menuPanels)
			{
				//Create the panel
				GameObject panel = Instantiate(menuPanel.panelPrefab, mainMenuPanel);
				panel.name = menuPanel.name;
				panel.SetActive(false);
				menuPanel.activePanel = panel;

				//If it has a MainMenuPanelBase, set it cancel button's onClick event to toggle it self
				if (panel.GetComponent<MainMenuPanelBase>() != null)
					panel.GetComponent<MainMenuPanelBase>().cancelButton.onClick.AddListener(delegate{TogglePanel(menuPanel.name);});

				//Create the button for it
				if (menuPanel.bottomNavBarButton)
				{
					CreateButton(bottomButtonPrefab, bottomNavBar, menuPanel.menuButtonText,
						delegate { TogglePanel(menuPanel.name); });
				}
				else
					CreateButton(topButtonPrefab, topNavBar, menuPanel.menuButtonText,
						delegate { TogglePanel(menuPanel.name); });
			}

			stopwatch.Stop();
			Logger.Info("Time taken to update menu UI: {@TotalMilliseconds}ms", stopwatch.ElapsedMilliseconds);

			tweeningManager.GetTweenObject("TopNavBar").PlayAllEvents();
			tweeningManager.GetTweenObject("BottomNavBar").PlayAllEvents();
		}

		private void Update()
		{
			//If the close key is pressed, then close the active panel
			if (Input.GetKeyDown(closeKey))
				CloseActivePanel();
		}

		private void OnDestroy()
		{
			Logger.Debug("Resetting all main menu events...");

			//Reset all the main menu script-able objects
			foreach (MainMenuPanel menu in menuPanels)
			{
				menu.isOpen = false;
				menu.activePanel = null;
			}
		}

		/// <summary>
		/// Toggles between panels
		/// </summary>
		/// <param name="panelName"></param>
		public void TogglePanel(string panelName)
		{
			MainMenuPanel panel = GetMenuPanel(panelName);

			//There is a panel that is currently active, so close it
			if (GetActivePanel() != null && panel != GetActivePanel())
			{
				Logger.Debug($"{GetActivePanel().name} is currently active, switching...");

				ClosePanel(GetActivePanel(), true);
			}

			if (!panel.isOpen)
				OpenPanel(panel);
			else
				ClosePanel(panel);
		}

		/// <summary>
		/// Closes the active panel
		/// </summary>
		public void CloseActivePanel()
		{
			if (GetActivePanel() != null)
				ClosePanel(GetActivePanel());
		}

		private void CreateButton(GameObject buttonPrefab, Transform parent, string text, UnityAction action, float maxSize = 52f, bool bold = false)
		{
			GameObject button = Instantiate(buttonPrefab, parent);
			TextMeshProUGUI tmpText = button.GetComponentInChildren<TextMeshProUGUI>();
			tmpText.text = text;
			tmpText.fontSizeMax = maxSize;
			if(bold)
				tmpText.fontStyle = FontStyles.Bold;
			button.GetComponent<Button>().onClick.AddListener(action);
			button.name = $"{text} Button";
		}

		#region Panel List Functions

		private MainMenuPanel GetMenuPanel(string panelName)
		{
			IEnumerable<MainMenuPanel> result = from a in menuPanels
				where a.name == panelName
				select a;

			return result.FirstOrDefault();
		}

		/// <summary>
		/// Returns the active panel's <see cref="MainMenuPanel"/>
		/// </summary>
		/// <returns></returns>
		public MainMenuPanel GetActivePanel()
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
			//Close the top black bar
			topBlackBarAnimator.SetTrigger(topBlackBarCloseTriggerName);
			yield return new WaitForSeconds(topBlackBarWaitTime);
			topBlackBarAnimator.gameObject.SetActive(false);
		}

		private void ActivateBlackBackground()
		{
			//If we are in game we want the black background always active
			if (NetworkManager.singleton.isNetworkActive)
				return;

			blackBackgroundAnimator.gameObject.SetActive(true);
		}

		private IEnumerator DeactivateBlackBackground()
		{
			//If we are in game we want the black background always active
			if (NetworkManager.singleton.isNetworkActive)
				yield break;

			//Close the black background
			blackBackgroundAnimator.SetTrigger(blackBackgroundCloseTriggerName);
			yield return new WaitForSeconds(blackBackgroundWaitTime);
			blackBackgroundAnimator.gameObject.SetActive(false);
		}

		#endregion

		#region Panel Functions

		private void ClosePanel(MainMenuPanel panel, bool isSwitching = false)
		{
			Logger.Debug($"Closing {panel.name}");

			if (!isSwitching)
			{
				if (panel.showTopBlackBar)
					StartCoroutine(DeactivateTopBlackBar());

				if (panel.darkenScreen)
					StartCoroutine(DeactivateBlackBackground());
			}

			panel.activePanel.SetActive(false);
			panel.isOpen = false;
		}

		private void OpenPanel(MainMenuPanel panel)
		{
			Logger.Debug($"Opening {panel.name}");

			if (panel.showTopBlackBar)
				ActivateTopBlackBar();

			if (panel.darkenScreen)
				ActivateBlackBackground();

			panel.activePanel.SetActive(true);
			panel.isOpen = true;
		}

		#endregion
	}
}