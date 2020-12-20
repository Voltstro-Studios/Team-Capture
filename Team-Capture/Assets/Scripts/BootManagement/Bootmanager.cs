using System.Collections;
using Team_Capture.Core;
using UnityEngine;

namespace Team_Capture.BootManagement
{
	internal class BootManager : MonoBehaviour
	{
		public float delayBetweenItems = 0.1f;
		public BootItem[] bootItems;

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