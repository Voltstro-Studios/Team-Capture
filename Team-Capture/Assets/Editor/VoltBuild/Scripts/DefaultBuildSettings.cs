using UnityEditor;
using UnityEngine;

namespace VoltBuilder
{
	public class DefaultBuildSettings : IBuildSettings
	{
		/// <inheritdoc/>
		public void DrawBuildSettings(BuildTool buildTool)
		{
			if (ConfigManager.GetBuildConfig(out DefaultBuildConfig config))
			{
				//Build Target Option
				config.BuildTarget =
					(BuildTarget) EditorGUILayout.EnumPopup("Build Target:", config.BuildTarget);

				//Dev build and copy PDB files options
				EditorGUILayout.BeginHorizontal();
				config.DevBuild = EditorGUILayout.Toggle("Dev Build", config.DevBuild);
				config.CopyPDBFiles = EditorGUILayout.Toggle("Copy PDB files", config.CopyPDBFiles);
				EditorGUILayout.EndHorizontal();

				//Server build(headless) and zip build options
				EditorGUILayout.BeginHorizontal();
				config.ServerBuild = EditorGUILayout.Toggle("Server Build", config.ServerBuild);
				config.ZipFiles = EditorGUILayout.Toggle("Zip Build", config.ZipFiles);
				EditorGUILayout.EndHorizontal();

				//Save settings button
				if (GUILayout.Button("Save Settings"))
					ConfigManager.SaveConfig();
			}
			else
				Debug.LogError("Build config is not the default one!");
		}
	}
}