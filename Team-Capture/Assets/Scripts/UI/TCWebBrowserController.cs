using Team_Capture.Console;
using UnityEngine;
using UnityWebBrowser;

namespace Team_Capture.UI
{
	[RequireComponent(typeof(WebBrowserUI))]
    public class TCWebBrowserController : MonoBehaviour
    {
		[ConVar("cl_webbrowser_ipc_port", "The port that the game and the process will communicate on")]
	    public static int WebBrowserPort = 5555;

	    [ConVar("cl_webbrowser_polling_time", "Time between each ping event sent")]
	    public static float WebBrowserPollingTime = 0.04f;

		[ConVar("cl_webbrowser_debug", "Enable debug logging for the web browser")]
	    public static bool WebBrowserDebugLog = false;

		[ConVar("cl_webbrowser_js", "Enable or disable JS for the web browser (WARNING, may break a lot of websites)")]
	    public static bool WebBrowserJs = true;

	    private void Start()
	    {
		    WebBrowserUI webBrowser = GetComponent<WebBrowserUI>();
		    webBrowser.browserClient.port = WebBrowserPort;
		    webBrowser.browserClient.debugLog = WebBrowserDebugLog;
		    webBrowser.browserClient.eventPollingTime = WebBrowserPollingTime;
		    webBrowser.browserClient.javascript = WebBrowserJs;
			Destroy(this);
	    }
    }
}