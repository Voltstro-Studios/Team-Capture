using UnityEditor;
using VoltBuilder;

namespace Editor.Scripts
{
	[InitializeOnLoad]
	public class VoltBuilderSetup
	{
		static VoltBuilderSetup()
		{
			BuildTool.SceneSettings = new TCSceneSettings();
		}
	}
}