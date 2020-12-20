using UnityEngine;

namespace Team_Capture.Helper
{
	/// <summary>
	///     Helpers for using Unity's layers
	/// </summary>
	public static class LayersHelper
	{
		/// <summary>
		///     Sets a object's and it children object's layer
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="newLayer"></param>
		public static void SetLayerRecursively(GameObject obj, int newLayer)
		{
			if (obj == null)
				return;

			obj.layer = newLayer;

			foreach (Transform child in obj.transform)
			{
				if (child == null)
					continue;

				SetLayerRecursively(child.gameObject, newLayer);
			}
		}
	}
}