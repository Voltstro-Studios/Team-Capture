using UnityEngine;
using UnityEngine.UI;

namespace Team_Capture.UI.LoadingScreen
{
	/// <summary>
	///     A panel for a loading screen
	/// </summary>
	internal class LoadingScreenUI : MonoBehaviour
	{
		[SerializeField] private Slider slider;

		/// <summary>
		///     Sets the loading bar amount
		/// </summary>
		/// <param name="amount"></param>
		public void SetLoadingBarAmount(float amount)
		{
			slider.value = amount;
		}
	}
}