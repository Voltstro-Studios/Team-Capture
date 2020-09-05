using System;
using System.Linq;
using ElRaccoone.Tweens;
using ElRaccoone.Tweens.Core;
using UnityEngine;
using UnityEngine.UI;
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
				objectToTween.SetActive(true);

				if (uiTweenEvent.moving)
				{
					Tween<float> moveTween = objectToTween.TweenAnchoredPositionY(uiTweenEvent.moveTo, uiTweenEvent.duration);
					moveTween.SetFrom(uiTweenEvent.moveFrom);
					moveTween.SetOnComplete(() => OnEnd(uiTweenEvent.activeOnEnd));
				}

				if (uiTweenEvent.fading)
				{
					Tween<float> fadeTween = objectToTween.GetComponent<Graphic>().TweenGraphicAlpha(uiTweenEvent.fadeTo, uiTweenEvent.duration);
					fadeTween.SetFrom(uiTweenEvent.fadeFrom);
					fadeTween.SetOnComplete(() => OnEnd(uiTweenEvent.activeOnEnd));
				}
			}
			else
			{
				Logger.Error("Unsupported tween event type!");
			}

			Logger.Debug("Played event {@Event}", tweenEvent.name);
		}

		private void OnEnd(bool activeOnEnd)
		{
			objectToTween.SetActive(activeOnEnd);
		}
	}
}