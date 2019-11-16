using UnityEditor;

public class TCSceneManagerGUI : EditorWindow
{
	[MenuItem("Team Capture/Scenes Manager")]
	public static void ShowWindow()
	{
		GetWindow<TCSceneManagerGUI>("Scenes Manager");
	}

	private void OnGUI()
	{
		//TODO: Find out how to display a list of all the scenes
	}
}
