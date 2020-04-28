using UnityEditor;
using UnityEditor.Build.Reporting;

namespace VoltBuilder
{
	public interface IGameBuild
	{
		/// <summary>
		/// Draws asset bundle GUI controls
		/// </summary>
		/// <param name="buildTool"></param>
		void DrawAssetBundleCommands(BuildTool buildTool);

		/// <summary>
		/// Builds the bundles
		/// </summary>
		/// <param name="buildPath"></param>
		/// <param name="options"></param>
		/// <param name="forced"></param>
		void BuildBundles(string buildPath, BuildAssetBundleOptions options, bool forced);

		/// <summary>
		/// Draw build game GUI controls
		/// </summary>
		/// <param name="buildTool"></param>
		void DrawBuildGameCommands(BuildTool buildTool);

		/// <summary>
		/// Builds the game
		/// </summary>
		/// <param name="levels"></param>
		/// <param name="buildPath"></param>
		/// <param name="fileName"></param>
		/// <param name="target"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		BuildReport BuildGame(string[] levels, string buildPath, string fileName, BuildTarget target, BuildOptions options);
	}
}