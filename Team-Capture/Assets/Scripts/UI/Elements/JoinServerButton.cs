using Core.Networking.Discovery;
using Mirror;
using TMPro;
using UnityEngine;

public class JoinServerButton : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI gameNameText;
	[SerializeField] private TextMeshProUGUI mapNameText;
	[SerializeField] private TextMeshProUGUI pingText;
	[SerializeField] private TextMeshProUGUI playerCountText;

    private string serverAddress;
    private int serverPort;

    public void SetupConnectButton(TCServerResponse server)
    {
	    serverAddress = server.EndPoint.Address.ToString();
	    serverPort = server.EndPoint.Port;

	    gameNameText.text = server.GameName;
	    mapNameText.text = server.SceneName;
	    pingText.text = "0";
	    playerCountText.text = $"{server.CurrentAmountOfPlayers}/{server.MaxPlayers}";
    }

    public void ConnectToServer()
    {
	    //TODO: Better connection stuff with Lite Net Lib 4 Mirror Transport
	    NetworkManager.singleton.networkAddress = serverAddress;
	    NetworkManager.singleton.StartClient();
    }
}
