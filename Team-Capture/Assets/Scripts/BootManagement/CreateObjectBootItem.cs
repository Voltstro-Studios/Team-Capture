using UnityEngine;

namespace Team_Capture.BootManagement
{
	/// <summary>
	///		Creates an object on boot
	/// </summary>
	[CreateAssetMenu(fileName = "Create Object Boot Item", menuName = "BootManager/Create Object Boot Item")]
    internal sealed class CreateObjectBootItem : BootItem
    {
		/// <summary>
		///		The object you want to create
		/// </summary>
		[Tooltip("The object you want to create")]
	    [SerializeField] private GameObject objectToCreate;

	    public override void OnBoot()
	    {
		    Instantiate(objectToCreate);
	    }
    }
}