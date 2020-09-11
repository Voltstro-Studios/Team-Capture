using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace BootManagement
{
	public class Bootmanager : MonoBehaviour
	{
		public static bool HasBooted;

		private void Awake()
		{
			//No needed to do stuff if this has been booted already
			if (HasBooted)
			{
				Destroy();
				return;
			}

			//First, find all scripts that inherit from IStartOnBoot
			IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
				.Where(x => typeof(IStartOnBoot).IsAssignableFrom(x) && !x.IsInterface);

			foreach (Type type in types)
			{
				try
				{
					//Create a new game object and add the script to it
					GameObject newBootedObject = new GameObject(type.Name);
					IStartOnBoot bootedScript = (IStartOnBoot) newBootedObject.AddComponent(type);
					bootedScript.Init();
				}
				catch
				{
					//Ignore
				}
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