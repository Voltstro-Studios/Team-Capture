using Team_Capture.Tweens;
using UnityEngine;
using UnityEngine.UI;

namespace Team_Capture.UI.Panels
{
	/// <summary>
	///     The base for a main menu panel
	/// </summary>
	internal class PanelBase : MonoBehaviour
	{
		/// <summary>
		///     The cancel or close button
		/// </summary>
		public Button cancelButton;

		/// <summary>
		///		The <see cref="TweenObject"/> for this panel
		/// </summary>
		public TweenObject tweenObject;

		public virtual void OnEnable()
		{
			if(tweenObject.objectToTween != null)
				tweenObject.PlayEvent("PanelOpen");
		}
	}
}