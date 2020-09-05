using UnityEngine;

namespace Tweens
{
	public enum UITweenStyle
	{
		Move = 1,

		Fade = 2
	}

	[CreateAssetMenu(fileName = "UITweenEvent", menuName = "Tweening/UITweenEvent" )]
	public class UITweenEvent : TweenEvent
	{
		//TODO: Write an editor script that changes depending on tweenStyle
		public UITweenStyle tweenStyle = UITweenStyle.Move;

		public float moveFrom = 1;
		public float moveTo;

		public float fadeFrom;
		public float fadeTo;

		public float duration;
	}
}