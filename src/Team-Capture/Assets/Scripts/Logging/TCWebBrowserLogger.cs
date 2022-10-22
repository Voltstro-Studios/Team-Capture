// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using VoltstroStudios.UnityWebBrowser.Logging;

namespace Team_Capture.Logging
{
    public class TCWebBrowserLogger : IWebBrowserLogger
    {
        public void Debug(object message)
        {
            Logger.Debug(message.ToString());
        }

        public void Warn(object message)
        {
            Logger.Warn(message.ToString());
        }

        public void Error(object message)
        {
            Logger.Error(message.ToString());
        }
    }
}