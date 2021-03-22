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

	    public static Cysharp.Threading.Tasks.UniTask Delay(int milliseconds)
	    {
		    return Cysharp.Threading.Tasks.UniTask.Delay(milliseconds);
	    }
    }
}