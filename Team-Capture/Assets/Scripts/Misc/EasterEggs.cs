using UnityEngine;
using UnityEngine.Diagnostics;
using Logger = Core.Logger.Logger;
using Random = System.Random;

namespace Misc
{
	[DefaultExecutionOrder(-2000)]
	public class EasterEggs : MonoBehaviour
	{
		private const string QuitMessage = "Don't go pressing keys when you don't know what they do";
		private static KeyCode[] quitKeys = {KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D};
		private static float quitAt = -1;

		[Range(1, 5)] [SerializeField] private int randomKeyLength = -1;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			if (randomKeyLength != -1)
			{
				quitKeys = new KeyCode[randomKeyLength];
				for (int i = 0; i < randomKeyLength; i++)
				{
					quitKeys[i] = (KeyCode) new Random().Next(60, 200);
					Debug.Log($"Quit key {i + 1} is {quitKeys[i]}");
				}
			}
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

		public void OnGUI()
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (quitAt == -1) return;
			//Draws a red box to cover up the screen
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), new Texture2D(1, 1), ScaleMode.StretchToFill, false, 1, Color.red, 0, 0);
			GUI.TextArea(new Rect(0, 0, Screen.width, Screen.height), QuitMessage, new GUIStyle
			{
				fontStyle = FontStyle.BoldAndItalic, fontSize = 100, wordWrap = true, alignment = TextAnchor.MiddleCenter
			});

			//Crash because fuck you that's why
			if (Time.unscaledTime > quitAt) Utils.ForceCrash(ForcedCrashCategory.Abort);
		}
	}
}