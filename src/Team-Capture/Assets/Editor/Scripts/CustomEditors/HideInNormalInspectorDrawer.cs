// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor.CustomEditors
{
    [CustomPropertyDrawer(typeof(HideInNormalInspector))]
    public class HideInNormalInspectorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return -2; // To compensate the gap between inspector's properties
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }
    }
}