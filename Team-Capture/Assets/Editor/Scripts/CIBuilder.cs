using UnityEditor;
using UnityEngine;
using Voltstro.UnityBuilder.Build;

namespace Team_Capture.Editor
{
    public static class CIBuilder
    {
        public static void StartVoltBuilder()
        {
	        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
	        string buildDir = $"{GameBuilder.GetBuildDirectory()}{target}-DevOpsBuild/";
	        GameBuilder.BuildGame(buildDir, target);
		}
    }
}