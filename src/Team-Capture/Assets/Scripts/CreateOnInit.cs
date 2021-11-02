// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;

namespace Team_Capture
{
    /// <summary>
    ///     Will create an object with the class on init
    ///     <para>Class must be partial!</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class CreateOnInit : Attribute
    {
        public string CallOnInit { get; set; }
        public string ObjectNameOverride { get; set; }
        public RuntimeInitializeLoadType LoadType { get; set; }
    }
}