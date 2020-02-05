using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Logger;
using Mirror;
using UI.Panels;
using UnityEngine;
using Logger = Core.Logger.Logger;

namespace UI
{
	public class MainMenuController : MonoBehaviour
	{
		[Header("Black Background")] public Animator blackBackgroundAnimator;
		public string blackBackgroundCloseTriggerName = "Exit";
		public float blackBackgroundWaitTime = 0.2f;
		public Transform mainMenuPanel;
		public TCMainMenuEvent[] menuPanels;

		[Header("Top Black Bar")] public Animator topBlackBarAnimator;
		public string topBlackBarCloseTriggerName = "Exit";
		public float topBlackBarWaitTime = 0.2f;

		public void Start()
		{
			topBlackBarAnimator.gameObject.SetActive(false);

			//Pre create all panels
			foreach (TCMainMenuEvent menuPanel in menuPanels)
			{
				GameObject panel = Instantiate(menuPanel.panelPrefab, mainMenuPanel);
				panel.name = menuPanel.name;
				panel.SetActive(false);
				menuPanel.activePanel = panel;

				//If the panel is a quit panel, then set its no button to toggle the panel
				if (panel.GetComponent<QuitPanel>() != null)
					panel.GetComponent<QuitPanel>().noBtn.onClick
						.AddListener(delegate { TogglePanel(menuPanel.name); });

				//Basically, the same thing above
				if (panel.GetComponent<CreateGamePanel>() != null)
					panel.GetComponent<CreateGamePanel>().cancelButton.onClick.AddListener(delegate
					{
						TogglePanel(menuPanel.name);
					});

				//Same as the above except for a disconnect panel
				if (panel.GetComponent<DisconnectPanel>() != null)
					panel.GetComponent<DisconnectPanel>().noBtn.onClick.AddListener(delegate
					{
						TogglePanel(menuPanel.name);
					});
			}
		}

		private void OnDestroy()
		{
			Logger.Log("Resetting all main menu events...", LogVerbosity.Debug);
			foreach (TCMainMenuEvent menu in menuPanels)
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
			TCMainMenuEvent panel = GetMenuPanel(panelName);

			//There is a panel that is currently active, so close it
			if (GetActivePanel() != null && panel != GetActivePanel())
			{
				Logger.Log($"{GetActivePanel().name} is currently active, switching...", LogVerbosity.Debug);

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

		private TCMainMenuEvent GetMenuPanel(string panelName)
		{
			IEnumerable<TCMainMenuEvent> result = from a in menuPanels
				where a.name == panelName
				select a;

			return result.FirstOrDefault();
		}

		private TCMainMenuEvent GetActivePanel()
		{
			IEnumerable<TCMainMenuEvent> result = from a in menuPanels
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
			if (NetworkManager.singleton.isNetworkActive) //If we are in game we want the black background always active
				return;

			blackBackgroundAnimator.gameObject.SetActive(true);
		}

		private IEnumerator DeactivateBlackBackground()
		{
			if (NetworkManager.singleton.isNetworkActive) //If we are in game we want the black background always active
				yield break;

			blackBackgroundAnimator.SetTrigger(blackBackgroundCloseTriggerName);
			yield return new WaitForSeconds(blackBackgroundWaitTime);
			blackBackgroundAnimator.gameObject.SetActive(false);
		}

		#endregion

		#region Panel Functions

		private void ClosePanel(TCMainMenuEvent panel, bool isSwitching = false)
		{
			Logger.Log($"Closing {panel.name}", LogVerbosity.Debug);

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

		private void OpenPanel(TCMainMenuEvent panel)
		{
			Logger.Log($"Opening {panel.name}", LogVerbosity.Debug);

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