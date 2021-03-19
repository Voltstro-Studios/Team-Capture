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
		/// <param name="config"></param>
		/// <param name="onClose"></param>
	    internal void Setup(ServerConfig config, Action onClose)
	    {
		    if (config.motdMode == Server.ServerMOTDMode.TextOnly)
		    {
			    if (string.IsNullOrWhiteSpace(config.motdText))
				    throw new InvalidMOTDSettings("The server's MOTD was set to text, however the sent over text contained nothing!");

				motdTextScroll.SetActive(true);
				webBrowserUI.gameObject.SetActive(false);

			    motdTitleText.text = $"{config.gameName}'s MOTD.";
			    motdText.text = config.motdText;
		    }

			else if (config.motdMode == Server.ServerMOTDMode.WebOnly)
		    {
				webBrowserUI.browserClient.ReplaceLogger(Logger.UnityLogger);
			    webBrowserUI.browserClient.initialUrl = config.motdUrl;

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