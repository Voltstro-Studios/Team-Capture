using Localization;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts.Localization
{
	[InitializeOnLoad]
	public class ForceSetLanguage : EditorWindow
	{
		static ForceSetLanguage()
		{
			InitLoadSettings();
		}

		private static LanguageInfo language = LanguageInfo.Unable;

		private static string LanguageEditorKey = "LocaleOverrideLanguage";

		[MenuItem("Localization/Set Language")]
		public static void SetLanguageMenu()
		{
			ForceSetLanguage window = CreateInstance<ForceSetLanguage>();
			window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 150);
			window.ShowPopup();
		}

		public static void InitLoadSettings()
		{
			if (EditorPrefs.HasKey(LanguageEditorKey))
			{
				language = (LanguageInfo) EditorPrefs.GetInt(LanguageEditorKey);
				if(language != LanguageInfo.Unable)
					SetLanguage(false);
			}
		}

		public void OnGUI()
		{
			EditorGUILayout.LabelField("Choose what language you want the game to use.");
			EditorGUILayout.LabelField("Select 'Unable' for system default.");
			EditorGUILayout.Space();

			language = (LanguageInfo)EditorGUILayout.EnumPopup("Language", language);

			if (GUILayout.Button("Set"))
			{
				SetLanguage();
				Close();
			}
		}

		public static void SetLanguage(bool saveSettings = true)
		{
			Locale.OverrideLanguage = language;
			Debug.Log($"Set locale override language to {Locale.OverrideLanguage}");

			if(saveSettings)
				EditorPrefs.SetInt(LanguageEditorKey, (int)language);
		}
	}
}