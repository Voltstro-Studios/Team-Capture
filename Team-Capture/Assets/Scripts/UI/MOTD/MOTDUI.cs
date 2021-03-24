using System;
using Cysharp.Threading.Tasks;
using Team_Capture.Core.Networking;
using Team_Capture.Core.UserAccount;
using TMPro;
using UnityEngine;
using UnityWebBrowser;
using Logger = Team_Capture.Logging.Logger;
using UniTask = Team_Capture.Integrations.UniTask.UniTask;

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
		    motdTitleText.text = $"{serverConfig.gameName}'s MOTD.";

		    if (serverConfig.motdMode == Server.ServerMOTDMode.WebOnly || serverConfig.motdMode == Server.ServerMOTDMode.WebWithTextBackup && Client.ClientMotdMode == Client.ClientMOTDMode.WebSupport)
		    {
			    webBrowserUI.browserClient.ReplaceLogger(Logger.UnityLogger);
			    webBrowserUI.browserClient.initialUrl = serverConfig.motdUrl;

			    motdTextScroll.SetActive(false);
			    webBrowserUI.gameObject.SetActive(true);
			    SendJs().Forget();
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

	    private async UniTaskVoid SendJs()
	    {
		    await UniTask.WaitUntil(() => webBrowserUI.browserClient.IsRunning);

		    string javaScriptCode =
			    $"class UserDetails {{ constructor(username) {{ this.UserName = username; }} }}" +
			    $"let userDetails = new UserDetails(\"{User.DefaultAccount.AccountName}\");";

		    webBrowserUI.ExecuteJs(javaScriptCode);
	    } 
    }
}