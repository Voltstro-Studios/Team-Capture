// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;

namespace Team_Capture.UI.MOTD
{
    public class InvalidMOTDSettings : Exception
    {
        public InvalidMOTDSettings(string message) : base(message)
        {
        }
    }
}