using UnityEngine;
using UnityEngine.Rendering;
using Logger = Core.Logger.Logger;

namespace Settings
{
	[RequireComponent(typeof(Volume))]
	public class VolumeDontDestroy : MonoBehaviour
	{
		private static VolumeDontDestroy instance;

		public static Volume ActiveVolume;

		private void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
				return;
			}

			instance = this;
			DontDestroyOnLoad(gameObject);

			ActiveVolume = GetComponent<Volume>();
		}

		private void Start()
		{
			Logger.Log("Volume settings are ready!");
		}
	}
}
