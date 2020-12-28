using System.Collections;
using Team_Capture.Core;
using UnityEngine;

namespace Team_Capture.BootManagement
{
	/// <summary>
	///		Handles creating and setting up stuff on boot
	/// </summary>
	internal class BootManager : MonoBehaviour
	{
		/// <summary>
		///		How long do you want to wait between each boot item start
		/// </summary>
		[Tooltip("How long do you want to wait between each boot item start")]
		[SerializeField] private float delayBetweenItems = 0.1f;

		/// <summary>
		///		All the boot items you want to do
		/// </summary>
		[Tooltip("All the boot items you want to do")]
		[SerializeField] private BootItem[] bootItems;

		private void Start()
		{
			StartCoroutine(RunBootItems());
		}

		private IEnumerator RunBootItems()
		{
			foreach (BootItem bootItem in bootItems)
			{
				if (Game.IsHeadless)
				{
					if(bootItem.runOn == RunOn.GraphicsOnly)
						continue;
				}
				else
				{
					if(bootItem.runOn == RunOn.ServerOnly)
						continue;
				}

				bootItem.OnBoot();

				yield return new WaitForSeconds(delayBetweenItems);
			}
		}
	}
}