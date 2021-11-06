// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Team_Capture.Helper
{
    /// <summary>
    ///     Helper for time
    /// </summary>
    public static class TimeHelper
    {
        /// <summary>
        ///     Gets Unix time now in total seconds
        /// </summary>
        /// <returns></returns>
        public static long UnixTimeNow()
        {
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long) timeSpan.TotalSeconds;
        }

        public static async UniTask CountDown(int time, Action<int> onTick = null,
            CancellationToken cancellationToken = default)
        {
            int currentCountDownTime = time;

            while (currentCountDownTime != 0)
            {
                await UniTask.Delay(1000, cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                currentCountDownTime--;
                onTick?.Invoke(currentCountDownTime);
            }
        }

        public static async UniTask CountUp(int counts, int milliseconds, Action<int> onTick = null,
            CancellationToken cancellationToken = default)
        {
            int currentCount = 0;
            while (currentCount != counts)
            {
                await UniTask.Delay(milliseconds, cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                currentCount++;
                onTick?.Invoke(currentCount);
            }
        }

        public static async UniTask InvokeRepeatedly(Action invoke, float repeatRate, CancellationToken cancellationToken)
        {
            int milliSeconds = (int)(repeatRate * 1000f);
            while (!cancellationToken.IsCancellationRequested)
            {
                invoke.Invoke();
                
                await UniTask.Delay(milliSeconds, cancellationToken: cancellationToken);
            }
        }
    }
}