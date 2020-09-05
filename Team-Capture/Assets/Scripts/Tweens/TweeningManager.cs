using System.Linq;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Tweens
{
	public class TweeningManager : MonoBehaviour
	{
		public TweenObject[] tweenedObjects;

		public TweenObject GetTweenObject(string tweenObjectName)
		{
			TweenObject tweenObject = tweenedObjects.FirstOrDefault(x => x.tweenObjectName == tweenObjectName);
			if (tweenObject == null)
			{
				Logger.Error("The tween object {@TweenObjectName) doesn't exist!", tweenObjectName);
				return null;
			}

			return tweenObject;
		}
	}
}