using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TCScene))]
public class TCSceneEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		TCScene scene = (TCScene) target;

		if (TCScenesManager.GetScene(scene.sceneName) == null)
		{
			if (GUILayout.Button("Add scene to manager"))
			{
				TCScenesManager.AddScene(scene);
				TCScenesManager.SaveScenes();
			}
		}

		if (GUILayout.Button("Force Save Scenes"))
		{
			TCScenesManager.SaveScenes();
		}
	}
}
