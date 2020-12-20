using UnityEngine;

namespace Team_Capture.BootManagement
{
	internal enum RunOn : byte
	{
        GraphicsOnly,
        ServerOnly,
        Both
	}

    internal class BootItem : ScriptableObject
    {
	    public RunOn runOn = RunOn.Both;

	    public virtual void OnBoot()
	    {
	    }
    }
}