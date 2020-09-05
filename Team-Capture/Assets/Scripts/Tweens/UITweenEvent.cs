using UnityEngine;

namespace Tweens
{
	[CreateAssetMenu(fileName = "UITweenEvent", menuName = "Tweening/UITweenEvent" )]
	public class UITweenEvent : TweenEvent
	{
		public bool moving = true;

		public float moveFrom = 1;
		public float moveTo;

		public bool fading = false;

		public float fadeFrom;
		public float fadeTo;
	}
}