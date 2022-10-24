// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Collections;
using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor.CustomEditors
{
    //From:
    //https://github.com/upscalebaby/generic-serializable-dictionary/blob/70133ea96240b5626711dc821039a84f05ca85e6/Assets/Editor/GenericDictionaryPropertyDrawer.cs
    
    /// <summary>
    /// Draws the dictionary and a warning-box if there are duplicate keys.
    /// </summary>
    [CustomPropertyDrawer(typeof(GenericDictionary<,>))]
    public class GenericDictionaryDrawer : PropertyDrawer
    {
        private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
        private static readonly float VertSpace = EditorGUIUtility.standardVerticalSpacing;
        private const float WarningBoxHeight = 1.5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Draw list of key/value pairs.
            SerializedProperty list = property.FindPropertyRelative("list");
            EditorGUI.PropertyField(position, list, label, true);

            // Draw key collision warning.
            bool keyCollision = property.FindPropertyRelative("keyCollision").boolValue;
            if (!keyCollision)
                return;
            
            position.y += EditorGUI.GetPropertyHeight(list, true);
            if (!list.isExpanded)
                position.y += VertSpace;
            
            position.height = LineHeight * WarningBoxHeight;
            position = EditorGUI.IndentedRect(position);
            
            EditorGUI.HelpBox(position, "Duplicate keys will not be serialized.", MessageType.Warning);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //Height of KeyValue list.
            float height = 0f;
            SerializedProperty list = property.FindPropertyRelative("list");
            height += EditorGUI.GetPropertyHeight(list, true);

            //Height of key collision warning.
            bool keyCollision = property.FindPropertyRelative("keyCollision").boolValue;
            if (!keyCollision)
                return height;
            
            height += WarningBoxHeight * LineHeight;
            if (!list.isExpanded)
                height += VertSpace;
            return height;
        }
    }
}
