// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Team_Capture.AddressablesAddons
{
    [Serializable]
    public class CachedLocalizedString
    {
        //TODO: On string update
        [SerializeField]
        private LocalizedString localizedString;

        private bool haveSetValue;
        private string cachedValue;

        public string Value
        {
            get
            {
                if (!haveSetValue)
                {
                    cachedValue = localizedString.GetLocalizedString();
                    haveSetValue = true;
                }

                return cachedValue!;
            }
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(CachedLocalizedString cachedLocalizedString) =>
            cachedLocalizedString.Value;
    }
}