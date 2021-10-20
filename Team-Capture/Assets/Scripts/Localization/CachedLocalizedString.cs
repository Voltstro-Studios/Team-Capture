// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine.Localization;

namespace Team_Capture.Localization
{
    [Serializable]
    public class CachedLocalizedString
    {
        //TODO: On string update
        public LocalizedString localizedString;

        private string cachedValue;

        public string Value => cachedValue ??= localizedString.GetLocalizedString();

        public override string ToString()
        {
            return Value;
        }
    }
}