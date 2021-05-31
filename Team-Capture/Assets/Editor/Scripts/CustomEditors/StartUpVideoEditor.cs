// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor.CustomEditors
{
	[CustomEditor(typeof(StartUpVideo.StartUpVideo))]
	public class StartUpVideoEditor : UnityEditor.Editor {

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			StartUpVideo.StartUpVideo vid = (StartUpVideo.StartUpVideo)target;

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