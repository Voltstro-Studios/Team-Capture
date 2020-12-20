using Team_Capture.SceneManagement;
using UnityEngine;

namespace Team_Capture.BootManagement
{
	[CreateAssetMenu(fileName = "Scene Boot Item", menuName = "BootManager/Scene Boot Item")]
    internal class LoadSceneBootItem : BootItem
    {
	    public TCScene sceneToLoadTo;

	    public override void OnBoot()
	    {
		    TCScenesManager.LoadScene(sceneToLoadTo);
	    }
    }
}