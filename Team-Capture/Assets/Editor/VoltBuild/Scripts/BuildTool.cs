using UnityEditor;
using UnityEngine;

namespace VoltBuilder
{
	public class BuildTool : EditorWindow
	{
		private string projectName;

		private ISceneSettings sceneSettings;
		private IBuildSettings buildSettings;
		private IGameBuild gameBuilder;

		[MenuItem("Tools/Volt Build/Build Tool")]
		public static void ShowWindow()
		{
			GetWindow(typeof(BuildTool), false, "Volt Build Tool");
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Volt Build Tool", EditorStyles.boldLabel);

			string inputName = EditorGUILayout.TextField("Project name: ", projectName);
			if (inputName != projectName)
			{
				projectName = inputName;
				ConfigManager.Config.ProjectName = inputName;
				ConfigManager.SaveConfig();
			}

			EditorGUILayout.Space();
			DrawSceneSettings();

			EditorGUILayout.Space();
			DrawBuildSettings();

			EditorGUILayout.Space();
			DrawBuildCommands();
		}

		private void OnEnable()
		{
			sceneSettings = new DefaultSceneSettings();
			buildSettings = new DefaultBuildSettings();
			gameBuilder = new DefaultGameBuild();

			projectName = ConfigManager.Config.ProjectName;
		}

		#region Scene Setting Stuff

		private void DrawSceneSettings()
		{
			EditorGUILayout.LabelField("Scene Settings", EditorStyles.boldLabel);
			sceneSettings.DrawSceneSettings(this);
		}

		#endregion

		#region Build Setting Stuff

		private void DrawBuildSettings()
		{
			EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			buildSettings.DrawBuildSettings(this);
		}

		#endregion

		#region Build Player Stuff

		private void DrawBuildCommands()
		{
			EditorGUILayout.LabelField("Build Commands", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			gameBuilder.DrawAssetBundleCommands(this);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Build Game");

			gameBuilder.DrawBuildGameCommands(this);
		}

		/// <summary>
		/// Gets the path where builds go to
		/// </summary>
		/// <returns></returns>
		public string GetBuildFolder()
		{
			return $"{Application.dataPath.Replace("Assets", "")}{ConfigManager.Config.BuildDir}";
		}

		#endregion
	}
}