using UnityEditor;
using UnityEngine;
using Voltstro.UnityBuilder.Build;
using Voltstro.UnityBuilder.Settings;

namespace Team_Capture.Editor
{
    public static class CIBuilder
    {
        public static void StartVoltBuilder()
        {
	        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

            //Yea, yea, we should update Volt Builder to have an API in GameBuilder to change targets, but its 3 AM and I just want this to work for now
	        UnityEditor.SettingsManagement.Settings voltSettings = SettingsManager.Instance;
            voltSettings.Set("BuildTarget", target);

			string buildDir = $"{GameBuilder.GetBuildDirectory()}{target}-DevOpsBuild/";

            GameBuilder.BuildGame(buildDir);
		}
    }
}