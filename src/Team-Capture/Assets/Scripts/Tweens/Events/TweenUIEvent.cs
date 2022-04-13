// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using ElRaccoone.Tweens;
using ElRaccoone.Tweens.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Team_Capture.Tweens.Events
{
    /// <summary>
    ///     A tween for a UI element
    /// </summary>
    [CreateAssetMenu(fileName = "UITweenEvent", menuName = "Team-Capture/Tweening/UITweenEvent")]
    internal class TweenUIEvent : TweenEventBase
    {
        /// <summary>
        ///     Is this tween moving?
        /// </summary>
        public bool moving = true;

        /// <summary>
        ///     Move from what position
        /// </summary>
        public float moveFrom = 1;

        /// <summary>
        ///     Move to what position
        /// </summary>
        public float moveTo;

        /// <summary>
        ///     Is this tween fading
        /// </summary>
        public bool fading;

        /// <summary>
        ///     Fade from what value
        /// </summary>
        public float fadeFrom;

        /// <summary>
        ///     Fade to what value
        /// </summary>
        public float fadeTo;

        public override void TweenPlay()
        {
            //If this is a moving tween event
            if (moving)
            {
                Tween<float> moveTween =
                    objectToTween.TweenAnchoredPositionY(moveTo, duration);
                moveTween.SetFrom(moveFrom);
                moveTween.SetOnComplete(onEnd);
            }

            //This is a fading tween event
            if (fading)
            {
                Tween<float> fadeTween = objectToTween.GetComponent<Graphic>()
                    .TweenGraphicAlpha(fadeTo, duration);
                fadeTween.SetFrom(fadeFrom);
                fadeTween.SetOnComplete(onEnd);
            }
        }
    }
}