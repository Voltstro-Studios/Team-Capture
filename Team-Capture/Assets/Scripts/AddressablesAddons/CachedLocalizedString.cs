// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine.Localization;

namespace Team_Capture.AddressablesAddons
{
    [Serializable]
    public class CachedLocalizedString
    {
        //TODO: On string update
        public LocalizedString localizedString;

        private string cachedValue;

        private bool isWaitingForLoad = false;

        public string Value
        {
            get
            {
                if (isWaitingForLoad)
                    return string.Empty;
                
                if (cachedValue == null)
                {
                    isWaitingForLoad = true;
                    cachedValue = localizedString.GetLocalizedString();
                    isWaitingForLoad = false;
                }
                    
                return cachedValue;
                
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }
}