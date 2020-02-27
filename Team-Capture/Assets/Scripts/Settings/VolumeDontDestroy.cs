using UnityEngine;
using Logger = Core.Logger.Logger;

namespace Settings
{
	public class VolumeDontDestroy : MonoBehaviour
	{
		private static VolumeDontDestroy instance;

		private void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
				return;
			}

			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			Logger.Log("Volume settings are ready!");
		}
	}
}
