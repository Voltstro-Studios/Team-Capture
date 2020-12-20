using System.Collections;
using Team_Capture.Core;
using UnityEngine;
using Logger = Team_Capture.Core.Logging.Logger;

namespace Team_Capture.BootManagement
{
	internal class BootManager : MonoBehaviour
	{
		public static bool HasBooted;

		public float delayBetweenItems = 0.1f;
		public BootItem[] bootItems;

		private void Start()
		{
			//No needed to do stuff if this has been booted already
			if (HasBooted)
			{
				Destroy();
				return;
			}

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

			Logger.Info("Bootloader has successfully loaded!");
			HasBooted = true;
			Destroy();
		}

		private void Destroy()
		{
			Destroy(gameObject);
		}
	}
}