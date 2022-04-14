// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;

namespace Team_Capture.Tweens.Events
{
    /// <summary>
    ///     The base for a tween event
    /// </summary>
    internal abstract class TweenEventBase : ScriptableObject
    {
        /// <summary>
        ///     The duration of the tween
        /// </summary>
        public float duration;

        /// <summary>
        ///     Will the gameobject being tweened be active on end
        /// </summary>
        public bool activeOnEnd = true;

        /// <summary>
        ///     Called when the tween needs to play
        /// </summary>
        public abstract void TweenPlay(GameObject objToTween, Action onEndAction);
    }
}