// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using NetFabric.Hyperlinq;
using Team_Capture.Core;
using Team_Capture.Tweens.Events;
using UnityEngine;
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
        public TweenEventBase[] eventsToPlay;

        public void Setup()
        {
            foreach (TweenEventBase tweenEvent in eventsToPlay)
                tweenEvent.TweenSetup(objectToTween, () => OnEnd(tweenEvent.activeOnEnd));
        }

        /// <summary>
        ///     Plays an event
        /// </summary>
        /// <param name="eventName"></param>
        public void PlayEvent(string eventName)
        {
            Option<TweenEventBase> tweenEvent = eventsToPlay.AsValueEnumerable().Where(x => x.name == eventName).First();
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
            foreach (TweenEventBase tweenEvent in eventsToPlay)
                PlayTween(tweenEvent);
        }

        private void PlayTween(TweenEventBase tweenEvent)
        {
            objectToTween.SetActive(true);
            tweenEvent.TweenPlay();

            if (!Game.IsGameQuitting)
                Logger.Debug("Played event {@Event}", tweenEvent.name);
        }

        private void OnEnd(bool activeOnEnd)
        {
            objectToTween.SetActive(activeOnEnd);
        }
    }
}