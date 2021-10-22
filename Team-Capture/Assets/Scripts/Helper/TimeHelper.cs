// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

		public static async UniTask CountDown(int time, Action<int> onTick = null, CancellationToken cancellationToken = default)
		{
			int currentCountDownTime = time;

			while (currentCountDownTime != 0)
			{
				await Integrations.UniTask.UniTask.Delay(1000, cancellationToken);
				
				if(cancellationToken.IsCancellationRequested)
					return;
				
				currentCountDownTime--;
				onTick?.Invoke(currentCountDownTime);
			}
		}

		public static async UniTask CountUp(int counts, int milliseconds, Action<int> onTick = null, CancellationToken cancellationToken = default)
		{
			int currentCount = 0;
			while (currentCount != counts)
			{
				await Integrations.UniTask.UniTask.Delay(milliseconds, cancellationToken);
				
				if(cancellationToken.IsCancellationRequested)
					return;
				
				currentCount++;
				onTick?.Invoke(currentCount);
			}
		}
	}
}