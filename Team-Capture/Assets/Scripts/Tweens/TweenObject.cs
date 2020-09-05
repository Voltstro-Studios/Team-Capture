using System;
using System.Linq;
using ElRaccoone.Tweens;
using ElRaccoone.Tweens.Core;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Tweens
{
	[Serializable]
	public class TweenObject
	{
		public string tweenObjectName;

		public GameObject objectToTween;

		public TweenEvent[] eventsToPlay;

		public void PlayEvent(string eventName)
		{
			TweenEvent tweenEvent = eventsToPlay.FirstOrDefault(x => x.name == eventName);
			if (tweenEvent == null)
			{
				Logger.Error("THere is no tween event called {@EventName} on tween object {@ObjectName}!", eventName, objectToTween.name);
				return;
			}

			PlayTween(tweenEvent);
		}

		public void PlayAllEvents()
		{
			foreach (TweenEvent tweenEvent in eventsToPlay)
			{
				PlayTween(tweenEvent);
			}
		}

		private void PlayTween(TweenEvent tweenEvent)
		{
			//For UI tween event
			if (tweenEvent is UITweenEvent uiTweenEvent)
			{
				switch (uiTweenEvent.tweenStyle)
				{
					case UITweenStyle.Move:
						Tween<float> moveTween = objectToTween.TweenAnchoredPositionY(uiTweenEvent.moveTo, uiTweenEvent.duration);
						moveTween.SetFrom(uiTweenEvent.moveFrom);
						break;
					case UITweenStyle.Fade:
						Tween<float> fadeTween = objectToTween.TweenGraphicAlpha(uiTweenEvent.fadeTo, uiTweenEvent.duration);
						fadeTween.SetFrom(uiTweenEvent.fadeFrom);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				Logger.Error("Unsupported tween event type!");
			}
		}
	}
}