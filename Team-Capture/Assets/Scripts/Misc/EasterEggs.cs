using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Logger = Core.Logger.Logger;

namespace Misc
{
	[DefaultExecutionOrder(-2000)]
	public class EasterEggs : MonoBehaviour
	{
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		private void Update()
		{
			if (!Input.GetKey(KeyCode.W)) return;
			if (!Input.GetKey(KeyCode.A)) return;
			if (!Input.GetKey(KeyCode.S)) return;
			if (!Input.GetKey(KeyCode.D)) return;
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (quitAt != -1) return;
			quitAt = Time.unscaledTime + 2f;
			Logger.Log(QuitMessage);
		}

		private static float quitAt = -1;
		private const string QuitMessage = "Don't go pressing keys when you don't know what they do";

		public void OnGUI()
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (quitAt == -1) return;
			if (Time.unscaledTime > quitAt)
			{
				
			}
#if UNITY_EDITOR
			GUI.TextArea(new Rect(0, 0, Screen.width, Screen.height), QuitMessage, EditorStyles.boldLabel);
#endif
		}
	}
}