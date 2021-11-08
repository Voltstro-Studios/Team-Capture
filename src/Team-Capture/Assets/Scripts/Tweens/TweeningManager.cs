// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using NetFabric.Hyperlinq;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Tweens
{
    /// <summary>
    ///     A manager for tweening
    /// </summary>
    internal class TweeningManager : MonoBehaviour
    {
        /// <summary>
        ///     What objects can be tweened
        /// </summary>
        public TweenObject[] tweenedObjects;

        /// <summary>
        ///     Gets a tweened object
        /// </summary>
        /// <param name="tweenObjectName"></param>
        /// <returns></returns>
        public TweenObject GetTweenObject(string tweenObjectName)
        {
            Option<TweenObject> result = tweenedObjects.AsValueEnumerable().Where(x => x.tweenObjectName == tweenObjectName).First();
            if (result.IsSome) 
                return result.Value;

            Logger.Error("The tween object {TweenObjectName) doesn't exist!", tweenObjectName);
            return null;
        }
    }
}