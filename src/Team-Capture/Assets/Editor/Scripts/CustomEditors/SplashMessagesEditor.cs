using System.IO;
using Team_Capture.Console.Fun;
using UnityEditor;
using UnityEngine;

namespace Team_Capture.Editor.CustomEditors
{
    [CustomEditor(typeof(SplashMessages))]
    public class SplashMessagesEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SplashMessages messages = (SplashMessages) target;

            if (GUILayout.Button("Import Messages"))
            {
                string path = EditorUtility.OpenFilePanel("Import Messages", "", "txt");
                if(path.Length == 0)
                    return;

                string[] lines = File.ReadAllLines(path);
                messages.messages = lines;
                
                if(GUI.changed)
                    EditorUtility.SetDirty(messages);
            }
        }
    }
}
