using System;
using Team_Capture.Core.Networking;
using TMPro;
using UnityEngine;
using UnityWebBrowser;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI.MOTD
{
	/// <summary>
	///		UI for displaying MOTDs
	/// </summary>
    internal class MOTDUI : MonoBehaviour
    {
	    [SerializeField] private TextMeshProUGUI motdTitleText;
	    [SerializeField] private TextMeshProUGUI motdText;

	    [SerializeField] private GameObject motdTextScroll;
	    [SerializeField] private WebBrowserUI webBrowserUI;

	    private Action onCloseAction;

		/// <summary>
		///		Setup the MOTD UI
		/// </summary>
		/// <param name="serverConfig"></param>
		/// <param name="onClose"></param>
	    internal void Setup(ServerConfig serverConfig, Action onClose)
	    {
		    if (serverConfig.motdMode == Server.ServerMOTDMode.TextOnly || serverConfig.motdMode == Server.ServerMOTDMode.WebWithTextBackup || Client.ClientMotdMode == Client.ClientMOTDMode.TextOnly)
		    {
			    if (string.IsNullOrWhiteSpace(serverConfig.motdText))
				    throw new InvalidMOTDSettings("The server's MOTD was set to text, however the sent over text contained nothing!");

				motdTextScroll.SetActive(true);
				webBrowserUI.gameObject.SetActive(false);

			    motdTitleText.text = $"{serverConfig.gameName}'s MOTD.";
			    motdText.text = serverConfig.motdText;
		    }

			if (serverConfig.motdMode == Server.ServerMOTDMode.WebOnly || serverConfig.motdMode == Server.ServerMOTDMode.WebWithTextBackup && Client.ClientMotdMode == Client.ClientMOTDMode.WebSupport)
		    {
				webBrowserUI.browserClient.ReplaceLogger(Logger.UnityLogger);
			    webBrowserUI.browserClient.initialUrl = serverConfig.motdUrl;

			    motdTextScroll.SetActive(false);
			    webBrowserUI.gameObject.SetActive(true);
		    }

		    onCloseAction = onClose;
	    }

	    public void CloseMOTD()
	    {
			onCloseAction.Invoke();
			Destroy(gameObject);
	    }
    }
}