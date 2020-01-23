using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
	public class LoadingScreenPanel : MonoBehaviour
	{
		[SerializeField] private Slider slider;

		public void SetLoadingBarAmount(float amount)
		{
			slider.value = amount;
		}
	}
}