using System.Collections.Generic;
using System.Linq;
using SceneManagement;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VoltBuilder;

namespace Editor.Scripts
{
	public class TCSceneSettings : ISceneSettings
	{
		private ReorderableList sceneList;

		public TCSceneSettings()
		{
			sceneList = CreateScenesList();
		}

		private ReorderableList CreateScenesList()
		{
			ReorderableList list = new ReorderableList(ConfigManager.Config.Scenes, typeof(Scene), true, true, false, false)
			{
				drawHeaderCallback = rect =>
				{
					EditorGUI.LabelField(rect, "Scenes");
				},
				drawElementCallback = (rect, index, active, focused) =>
				{
					EditorGUI.LabelField(rect, (sceneList.list as List<Scene>)?[index].SceneName);
				},
				onReorderCallback = reorderableList =>
				{
					ConfigManager.Config.Scenes = (List<Scene>)reorderableList.list;
					ConfigManager.SaveConfig();
				}
			};

			sceneList = list;
			return list;
		}

		private Scene[] GetAllEnabledScenes()
		{
			List<Scene> scenes = new List<Scene>();
			foreach (TCScene scene in TCScenesManager.GetAllEnabledTCScenesInfo())
			{
				scenes.Add(new Scene
				{
					SceneLocation = scene.scene,
					SceneName = scene.displayName
				});
			}

			return scenes.ToArray();
		}

		private void ReloadScenes()
		{
			sceneList.list = GetAllEnabledScenes().ToList();
			ConfigManager.Config.Scenes = (List<Scene>)sceneList.list;
			ConfigManager.SaveConfig();
		}

		/// <inheritdoc/>
		public void DrawSceneSettings()
		{
			sceneList.DoLayoutList();

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Reload Scene List"))
				ReloadScenes();

			if (GUILayout.Button("Build Settings"))
				EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"), true);

			GUILayout.EndHorizontal();
		}
	}
}