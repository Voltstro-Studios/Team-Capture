#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace StartUpVideo.Editor
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

#endif