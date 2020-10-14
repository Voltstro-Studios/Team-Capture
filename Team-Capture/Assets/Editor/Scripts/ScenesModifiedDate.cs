using System.IO;
using UnityEditor;
using UnityEngine;
using Voltstro.UnityBuilder.Actions;

namespace Editor.Scripts
{
	public class ScenesModifiedDate : IBuildAction
	{
		public void OnGUI()
		{
		}

		public void OnBeforeBuild(string buildLocation)
		{
		}

		public void OnAfterBuild(string buildLocation)
		{
			string dataLocation = $"{buildLocation}/{Application.productName}_Data/";
			Debug.Log(dataLocation);

			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			for (int i = 0; i < scenes.Length; i++)
			{
				string level = $"{dataLocation}level{i}";
				if (File.Exists(scenes[i].path) && File.Exists(level))
				{
					File.SetLastWriteTime(level, File.GetLastWriteTime(scenes[i].path));
				}
			}
		}
	}
}