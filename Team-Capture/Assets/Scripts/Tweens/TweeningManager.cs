using System.Linq;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Tweens
{
	/// <summary>
	///     A manager for tweening
	/// </summary>
	internal class TweeningManager : MonoBehaviour
	{
		/// <summary>
		///     What objects can be tweened
		/// </summary>
		public TweenObject[] tweenedObjects;

		/// <summary>
		///     Gets a tweened object
		/// </summary>
		/// <param name="tweenObjectName"></param>
		/// <returns></returns>
		public TweenObject GetTweenObject(string tweenObjectName)
		{
			TweenObject tweenObject = tweenedObjects.FirstOrDefault(x => x.tweenObjectName == tweenObjectName);
			if (tweenObject != null) return tweenObject;

			Logger.Error("The tween object {@TweenObjectName) doesn't exist!", tweenObjectName);
			return null;
		}
	}
}