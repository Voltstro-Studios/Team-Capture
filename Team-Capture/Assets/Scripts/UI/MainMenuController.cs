using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UI.Panels;
using UnityEngine;
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

		private void Start()
		{
			topBlackBarAnimator.gameObject.SetActive(false);

			//Pre-create all panels
			foreach (MainMenuPanel menuPanel in menuPanels)
			{
				GameObject panel = Instantiate(menuPanel.panelPrefab, mainMenuPanel);
				panel.name = menuPanel.name;
				panel.SetActive(false);
				menuPanel.activePanel = panel;

				//If it has a MainMenuPanelBase, set it cancel button's onClick event to toggle it self
				if (panel.GetComponent<MainMenuPanelBase>() != null)
					panel.GetComponent<MainMenuPanelBase>().cancelButton.onClick.AddListener(delegate{TogglePanel(menuPanel.name);});
			}
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