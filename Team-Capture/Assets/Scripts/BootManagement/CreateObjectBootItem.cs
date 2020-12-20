using UnityEngine;

namespace Team_Capture.BootManagement
{
	[CreateAssetMenu(fileName = "Create Object Boot Item", menuName = "BootManager/Create Object Boot Item")]
    internal class CreateObjectBootItem : BootItem
    {
	    public GameObject objectToCreate;

	    public override void OnBoot()
	    {
		    Instantiate(objectToCreate);
	    }
    }
}