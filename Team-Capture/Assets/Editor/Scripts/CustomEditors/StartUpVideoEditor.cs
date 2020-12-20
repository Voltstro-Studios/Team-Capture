using UnityEditor;
using UnityEngine;
using Team_Capture.StartUpVideo;

namespace Editor.Scripts.CustomEditors
{
	[CustomEditor(typeof(StartUpVideo))]
	public class StartUpVideoEditor : UnityEditor.Editor {

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			StartUpVideo vid = (StartUpVideo)target;

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Setup Now"))
			{
				vid.Setup();
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}