using System;
using System.Collections;
using Team_Capture.Core.Networking;
using Team_Capture.Player;
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
		///		This javascript code will provide the client a way to display info such as username on the webpage
		///		<para>
		///			You can do something like 'document.getElementById("playerName").innerHTML = userDetails.UserName;' to set
		///			text to the player's name.
		///		</para>
		/// </summary>
	    private readonly string javaScriptCode =
		    $"class UserDetails {{ constructor(username) {{ this.UserName = username; }} }}" +
		    $"let userDetails = new UserDetails(\"{PlayerManager.StartPlayerName}\");";

		/// <summary>
		///		Setup the MOTD UI
		/// </summary>
		/// <param name="serverConfig"></param>
		/// <param name="onClose"></param>
	    internal void Setup(ServerConfig serverConfig, Action onClose)
	    {
		    motdTitleText.text = $"{serverConfig.gameName}'s MOTD.";

		    if (serverConfig.motdMode == Server.ServerMOTDMode.WebOnly || serverConfig.motdMode == Server.ServerMOTDMode.WebWithTextBackup && Client.ClientMotdMode == Client.ClientMOTDMode.WebSupport)
		    {
			    webBrowserUI.browserClient.ReplaceLogger(Logger.UnityLogger);
			    webBrowserUI.browserClient.initialUrl = serverConfig.motdUrl;

			    motdTextScroll.SetActive(false);
			    webBrowserUI.gameObject.SetActive(true);
			    StartCoroutine(SendJs());
		    }

		    else if (serverConfig.motdMode == Server.ServerMOTDMode.TextOnly || serverConfig.motdMode == Server.ServerMOTDMode.WebWithTextBackup)
		    {
			    if (string.IsNullOrWhiteSpace(serverConfig.motdText))
				    throw new InvalidMOTDSettings("The server's MOTD was set to text, however the sent over text contained nothing!");

				motdTextScroll.SetActive(true);
				webBrowserUI.gameObject.SetActive(false);

			    motdText.text = serverConfig.motdText;
		    }

		    onCloseAction = onClose;
	    }

	    public void CloseMOTD()
	    {
			onCloseAction.Invoke();
			Destroy(gameObject);
	    }

	    private IEnumerator SendJs()
	    {
		    //TODO: Make webBrowserClient.isRunning public
		    yield return new WaitForSeconds(1.0f);
		    webBrowserUI.ExecuteJs(javaScriptCode);
	    } 
    }
}