// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Cysharp.Threading.Tasks;
using Team_Capture.Core.Networking;
using Team_Capture.UserManagement;
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
		    motdTitleText.text = $"{serverConfig.GameName.String}'s MOTD.";

		    if (serverConfig.MotdMode == Server.ServerMOTDMode.WebOnly || serverConfig.MotdMode == Server.ServerMOTDMode.WebWithTextBackup && Client.ClientMotdMode == Client.ClientMOTDMode.WebSupport)
		    {
			    webBrowserUI.browserClient.ReplaceLogger(Logger.UnityLogger);
			    webBrowserUI.browserClient.initialUrl = serverConfig.MotdUrl.String;

			    motdTextScroll.SetActive(false);
			    webBrowserUI.gameObject.SetActive(true);
			    SendJs().Forget();
		    }

		    else if (serverConfig.MotdMode == Server.ServerMOTDMode.TextOnly || serverConfig.MotdMode == Server.ServerMOTDMode.WebWithTextBackup)
		    {
			    if (string.IsNullOrWhiteSpace(serverConfig.MotdText.String))
				    throw new InvalidMOTDSettings("The server's MOTD was set to text, however the sent over text contained nothing!");

				motdTextScroll.SetActive(true);
				webBrowserUI.gameObject.SetActive(false);

			    motdText.text = serverConfig.MotdText.String;
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
			    $"let userDetails = new UserDetails(\"{User.GetActiveUser().UserName}\");";

		    webBrowserUI.ExecuteJs(javaScriptCode);
	    } 
    }
}