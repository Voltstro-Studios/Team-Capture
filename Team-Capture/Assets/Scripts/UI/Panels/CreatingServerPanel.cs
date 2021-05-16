using Team_Capture.Localization;
using TMPro;
using UnityEngine;

namespace Team_Capture.UI.Panels
{
	/// <summary>
	///		A panel that is displayed when the server is starting
	/// </summary>
	internal class CreatingServerPanel : PanelBase
	{
		/// <summary>
		///		Whats the locale to use for the starting message
		/// </summary>
		[Tooltip("Whats the locale to use for the starting message")]
		public string startingMessageLocale = "Menu_StartingServer";

		/// <summary>
		///		Whats the locale to use for the failed to start message
		/// </summary>
		[Tooltip("Whats the locale to use for the failed to start message")]
		public string failedToStartMessageLocale = "Menu_StartingServerFail";

		public string failedToConnectMessageLocale = "Menu_ConnectFail";

		/// <summary>
		///		The text object of where the text will be placed
		/// </summary>
		public TMP_Text messageText;

		private void Start()
		{
			cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
		}

		public override void OnEnable()
		{
			base.OnEnable();
			cancelButton.interactable = false;
			messageText.text = GameUILocale.ResolveString(startingMessageLocale);
		}

		/// <summary>
		///		Call this when the server process fails to start
		/// </summary>
		public void FailedToStartMessage()
		{
			cancelButton.interactable = true;
			messageText.text = GameUILocale.ResolveString(failedToStartMessageLocale);
		}

		/// <summary>
		///		Call this when the client fails to connect to the started server
		/// </summary>
		public void FailedToConnectMessage()
		{
			cancelButton.interactable = true;
			messageText.text = GameUILocale.ResolveString(failedToConnectMessageLocale);
		}
	}
}