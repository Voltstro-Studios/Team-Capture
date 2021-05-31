// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Localization;
using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor.Localization
{
	[InitializeOnLoad]
	public class ForceSetLanguage : EditorWindow
	{
		static ForceSetLanguage()
		{
			InitLoadSettings();
		}

		private static LanguageInfo language = LanguageInfo.Unavailable;

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
				if(language != LanguageInfo.Unavailable)
					SetLanguage(false);
			}
		}

		public void OnGUI()
		{
			EditorGUILayout.LabelField("Choose what language you want the game to use.");
			EditorGUILayout.LabelField("Select 'Unavailable' for system default.");
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