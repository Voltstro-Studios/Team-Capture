// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using ElRaccoone.Tweens;
using NetFabric.Hyperlinq;
using Team_Capture.Core;
using UnityEngine;
using UnityEngine.UI;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Tweens
{
    /// <summary>
    ///     A class for handling tweening
    /// </summary>
    [Serializable]
    internal class TweenObject
    {
        /// <summary>
        ///     The name of this tweened object
        /// </summary>
        public string tweenObjectName;

        /// <summary>
        ///     The object to tween
        /// </summary>
        public GameObject objectToTween;

        /// <summary>
        ///     What events can we play?
        /// </summary>
        public TweenEvent[] eventsToPlay;

        /// <summary>
        ///     Plays an event
        /// </summary>
        /// <param name="eventName"></param>
        public void PlayEvent(string eventName)
        {
            Option<TweenEvent> tweenEvent = eventsToPlay.AsValueEnumerable().Where(x => x.name == eventName).First();
            if (tweenEvent.IsNone)
            {
                Logger.Error("There is no tween event called {EventName} on tween object {ObjectName}!", eventName,
                    objectToTween.name);
                return;
            }

            PlayTween(tweenEvent.Value);
        }

        /// <summary>
        ///     Plays all events
        /// </summary>
        public void PlayAllEvents()
        {
            foreach (TweenEvent tweenEvent in eventsToPlay)
                PlayTween(tweenEvent);
        }

        private void PlayTween(TweenEvent tweenEvent)
        {
            //For UI tween event
            if (tweenEvent is UITweenEvent uiTweenEvent)
            {
                objectToTween.SetActive(true);

                //If this is a moving tween event
                if (uiTweenEvent.moving)
                {
                    var moveTween =
                        objectToTween.TweenAnchoredPositionY(uiTweenEvent.moveTo, uiTweenEvent.duration);
                    moveTween.SetFrom(uiTweenEvent.moveFrom);
                    moveTween.SetOnComplete(() => OnEnd(uiTweenEvent.activeOnEnd));
                }

                //This is a fading tween event
                if (uiTweenEvent.fading)
                {
                    var fadeTween = objectToTween.GetComponent<Graphic>()
                        .TweenGraphicAlpha(uiTweenEvent.fadeTo, uiTweenEvent.duration);
                    fadeTween.SetFrom(uiTweenEvent.fadeFrom);
                    fadeTween.SetOnComplete(() => OnEnd(uiTweenEvent.activeOnEnd));
                }
            }
            else
            {
                Logger.Error("Unsupported tween event type!");
            }

            if (!Game.IsGameQuitting)
                Logger.Debug("Played event {@Event}", tweenEvent.name);
        }

        private void OnEnd(bool activeOnEnd)
        {
            objectToTween.SetActive(activeOnEnd);
        }
    }
}