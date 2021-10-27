// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.IO;
using Team_Capture.Console;
using UnityEngine;
using UnityWebBrowser;
using UnityWebBrowser.Shared;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI
{
    [RequireComponent(typeof(WebBrowserUI))]
    public class TCWebBrowserController : MonoBehaviour
    {
        //[ConVar("cl_webbrowser_ipc_port", "The port that the game and the process will communicate on", true)]
        //public static int WebBrowserPort = 5555;

        //[ConVar("cl_webbrowser_polling_time", "Time between each ping event sent", true)]
        //public static float WebBrowserPollingTime = 0.04f;

        [ConVar("cl_webbrowser_debug", "Enable debug logging for the web browser", true)]
        public static bool WebBrowserDebugLog = false;

        [ConVar("cl_webbrowser_js", "Enable or disable JS for the web browser (WARNING, may break a lot of websites)",
            true)]
        public static bool WebBrowserJs = true;

        [ConVar("cl_webbrowser_cache", "Enable or disable the cache", true)]
        public static bool WebBrowserCache = true;

        [ConVar("cl_webbrowser_proxy", "Enable or disable the proxy server", true)]
        public static bool WebBrowserProxy = true;

        [ConVar("cl_webbrowser_proxy_username", "The username for the proxy", true)]
        public static string WebBrowserProxyUsername = string.Empty;

        [ConVar("cl_webbrowser_proxy_password", "The password for the proxy", true)]
        public static string WebBrowserProxyPassword = string.Empty;

        private void Start()
        {
            WebBrowserUI webBrowser = GetComponent<WebBrowserUI>();
            webBrowser.browserClient.logSeverity = WebBrowserDebugLog ? LogSeverity.Debug : LogSeverity.Info;
            webBrowser.browserClient.javascript = WebBrowserJs;
            webBrowser.browserClient.cache = WebBrowserCache;
            webBrowser.browserClient.LogPath = new FileInfo($"{Logger.LoggerConfig.LogDirectory}/cef.log");
            webBrowser.browserClient.proxySettings.ProxyServer = !WebBrowserProxy;
            webBrowser.browserClient.proxySettings.Username = WebBrowserProxyUsername;
            webBrowser.browserClient.proxySettings.Password = WebBrowserProxyPassword;
            Destroy(this);
        }
    }
}