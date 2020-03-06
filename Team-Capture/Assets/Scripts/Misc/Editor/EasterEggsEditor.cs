using UnityEditor;

namespace Misc
{
	[CustomEditor(typeof(EasterEggs))]
	public class EasterEggsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("Well done for finding this script!", MessageType.None);
			base.OnInspectorGUI();
		}
	}
}