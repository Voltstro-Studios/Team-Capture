// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Team_Capture.Console;
using Team_Capture.Logging;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Team_Capture.AddressablesAddons
{
    internal static class AddressableCommands
    {
        [ConCommand("cl_locale_get", "Gets the value of localized string", CommandRunPermission.Both, 2, 2)]
        public static void GetLocalizedString(string[] args)
        {
            TableReference tableReference = args[0];
            TableEntryReference tableEntryReference = args[1];
            LocalizedString localString = new LocalizedString(tableReference, tableEntryReference);

            try
            {
                string value = localString.GetLocalizedString();
                Logger.Info("Got localized string with a value of: '{Value}'", value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get localized string! Reference probably doesn't exist!");
            }
        }
    }
}