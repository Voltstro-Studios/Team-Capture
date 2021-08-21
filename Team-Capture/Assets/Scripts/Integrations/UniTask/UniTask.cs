// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Team_Capture.Logging;
using UnityEngine.LowLevel;

namespace Team_Capture.Integrations.UniTask
{
    public static class UniTask
    {
	    static UniTask()
	    {
		    if (!PlayerLoopHelper.IsInjectedUniTaskPlayerLoop())
		    {
			    Logger.Debug("Injecting player loop...");
			    PlayerLoopSystem loop = PlayerLoop.GetCurrentPlayerLoop();
			    PlayerLoopHelper.Initialize(ref loop);
		    }
	    }

	    public static Cysharp.Threading.Tasks.UniTask Delay(int milliseconds, CancellationToken cancellationToken = default)
	    {
		    return Cysharp.Threading.Tasks.UniTask.Delay(milliseconds, cancellationToken: cancellationToken);
	    }

	    public static Cysharp.Threading.Tasks.UniTask WaitUntil(Func<bool> predicate)
	    {
		    return Cysharp.Threading.Tasks.UniTask.WaitUntil(predicate);
	    }
    }
}