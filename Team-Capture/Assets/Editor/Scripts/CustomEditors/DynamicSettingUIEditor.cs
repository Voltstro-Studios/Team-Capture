using Team_Capture.UI;
using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor.CustomEditors
{
    [CustomEditor(typeof(DynamicSettingsUI))]
    public class DynamicSettingUIEditor : UnityEditor.Editor
    {
	    public override void OnInspectorGUI()
	    {
		    base.OnInspectorGUI();

		    DynamicSettingsUI settingsUi = (DynamicSettingsUI)target;

		    EditorGUILayout.Space();

			if(GUILayout.Button("Update UI"))
				settingsUi.UpdateUI();
	    }
    }
}