// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Tweens
{
	/// <summary>
	///     A tween for a UI element
	/// </summary>
	[CreateAssetMenu(fileName = "UITweenEvent", menuName = "Tweening/UITweenEvent")]
	internal class UITweenEvent : TweenEvent
	{
		/// <summary>
		///     Is this tween moving?
		/// </summary>
		public bool moving = true;

		/// <summary>
		///     Move from what position
		/// </summary>
		public float moveFrom = 1;

		/// <summary>
		///     Move to what position
		/// </summary>
		public float moveTo;

		/// <summary>
		///     Is this tween fading
		/// </summary>
		public bool fading;

		/// <summary>
		///     Fade from what value
		/// </summary>
		public float fadeFrom;

		/// <summary>
		///     Fade to what value
		/// </summary>
		public float fadeTo;
	}
}