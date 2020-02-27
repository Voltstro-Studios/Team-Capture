using UnityEngine;

namespace Settings
{
	public class VolumeDontDestroy : MonoBehaviour
	{
		private VolumeDontDestroy Instance;

		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
}
