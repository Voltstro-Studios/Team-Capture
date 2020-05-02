using UnityEditor;
using UnityEngine;

namespace VoltBuilder
{
	public class CopyFilesWindow : EditorWindow
	{
		[MenuItem("Tools/Volt Build/Copy Files On Build Editor")]
		public static void ShowWindow()
		{
			if(ConfigManager.GetBuildConfig(out DefaultBuildConfig _))
				GetWindow(typeof(CopyFilesWindow), false, "Copy Files On Build Editor");
			else
				Debug.LogError("The build config that is used cannot be used with this Copy Files Editor!");
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Copy Files On Build Editor", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("These files will be copied to the build folder");
			EditorGUILayout.LabelField("after build (with default VoltBuilder).");
			EditorGUILayout.Space();

			if (ConfigManager.GetBuildConfig(out DefaultBuildConfig config))
			{
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField("File to copy");
				EditorGUILayout.LabelField("Where to (in build)");

				EditorGUILayout.EndHorizontal();

				for (int i = 0; i < config.FilesToCopyOnBuild.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();

					config.FilesToCopyOnBuild[i].WhatFileToCopy =
						EditorGUILayout.TextField(config.FilesToCopyOnBuild[i].WhatFileToCopy);

					config.FilesToCopyOnBuild[i].CopyToWhere =
						EditorGUILayout.TextField(config.FilesToCopyOnBuild[i].CopyToWhere);

					if (GUILayout.Button("-"))
						config.FilesToCopyOnBuild.Remove(config.FilesToCopyOnBuild[i]);

					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.Space();

				EditorGUILayout.BeginHorizontal();

				if (GUILayout.Button("Add New"))
				{
					config.FilesToCopyOnBuild.Add(new FileToCopy
					{
						WhatFileToCopy = "Assets/Example.txt",
						CopyToWhere = "Example.txt"
					});
				}

				if(GUILayout.Button("Save"))
					ConfigManager.SaveConfig();

				EditorGUILayout.EndHorizontal();
			}
			else
				EditorGUILayout.LabelField("The build config that is used cannot be used with this Copy Files Editor!");
		}
	}
}