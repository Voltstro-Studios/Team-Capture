using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BootManagement
{
	public class Bootmanager : MonoBehaviour
	{
		private void Awake()
		{
			//First, find all scripts that inherit from IStartOnBoot
			IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
				.Where(x => typeof(IStartOnBoot).IsAssignableFrom(x) && !x.IsInterface);

			foreach (Type type in types)
			{
				GameObject newBootedObject = new GameObject(type.Name);
				IStartOnBoot bootedScript = (IStartOnBoot)newBootedObject.AddComponent(type);
				bootedScript.Init();

				Debug.Log(type.Name);
			}
		}
	}
}