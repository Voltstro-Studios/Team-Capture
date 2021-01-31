using System;
using Team_Capture.Core.Networking;
using Team_Capture.Settings.Enums;
using TMPro;
using UnityEngine;

namespace Team_Capture.UI.MOTD
{
    internal class MOTDUI : MonoBehaviour
    {
	    [SerializeField] private TextMeshProUGUI motdTitleText;
	    [SerializeField] private TextMeshProUGUI motdText;

	    private Action onCloseAction;

	    internal void Setup(ServerConfig config, Action onClose)
	    {
		    if (config.motdMode == Server.ServerMOTDMode.TextOnly)
		    {
			    if (string.IsNullOrWhiteSpace(config.motdText))
				    throw new InvalidMOTDSettings("The server's MOTD was set to text, however the sent over text contained nothing!");

			    motdTitleText.text = $"{config.gameName}'s MOTD.";
			    motdText.text = config.motdText;
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