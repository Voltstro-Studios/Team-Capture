using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VoltBuilder
{
	public class DefaultGameBuild : IGameBuild
	{
		/// <inheritdoc/>
		public void DrawAssetBundleCommands(BuildTool buildTool)
		{
			EditorGUILayout.LabelField("Asset Bundles");
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Build Bundles"))
				BuildBundles($"{buildTool.GetBuildFolder()}/Bundles", BuildAssetBundleOptions.UncompressedAssetBundle, false);

			if (GUILayout.Button("Force Build Bundles"))
				BuildBundles($"{buildTool.GetBuildFolder()}/Bundles", BuildAssetBundleOptions.UncompressedAssetBundle, true);

			GUILayout.EndHorizontal();
		}

		/// <inheritdoc/>
		public void BuildBundles(string buildPath, BuildAssetBundleOptions options, bool forced)
		{
			//Create the directory if it doesn't exist
			if (!Directory.Exists(buildPath))
				Directory.CreateDirectory(buildPath);

			//Setup options
			if (forced)
				options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

			//Build asset bundles
			BuildPipeline.BuildAssetBundles(buildPath, options, BuildTarget.StandaloneWindows64);
		}

		/// <inheritdoc/>
		public void DrawBuildGameCommands(BuildTool buildTool)
		{
			//Build game commands
			GUILayout.BeginHorizontal();

			if(GUILayout.Button("Build Game"))
				DoGameBuild($"{buildTool.GetBuildFolder()}", $"{ConfigManager.Config.ProjectName}-Quick/", $"{ConfigManager.Config.ProjectName}", false);
			if(GUILayout.Button("Scripts only"))
				DoGameBuild($"{buildTool.GetBuildFolder()}", $"{ConfigManager.Config.ProjectName}-Quick/", $"{ConfigManager.Config.ProjectName}", true);

			GUILayout.EndHorizontal();

			//New build commands, as well as open to the build folder (Default `/Build`)
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Full New Build"))
				FullNewBuild(buildTool.GetBuildFolder(), ConfigManager.Config.ProjectName);

			if (GUILayout.Button("Open Build Folder"))
			{
				if (Directory.Exists(buildTool.GetBuildFolder()))
				{
					Process p = new Process
					{
						StartInfo = new ProcessStartInfo("explorer.exe", buildTool.GetBuildFolder().Replace("/", "\\"))
					};
					p.Start();
				}
				else
					Debug.LogError("Build folder doesn't exist yet!");
			}

			GUILayout.EndHorizontal();
		}

		/// <inheritdoc/>
		public BuildReport BuildGame(string[] levels, string buildPath, string fileName, BuildTarget target, BuildOptions options)
		{
			if (!Directory.Exists(buildPath))
				Directory.CreateDirectory(buildPath);

			//Since it is a Windows build, we need to add '.exe' to the end of it
			if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
				fileName += ".exe";

			//Build the game, and return its result
			return BuildPipeline.BuildPlayer(levels, buildPath + fileName, target, options);
		}

		#region Private Methods

		/// <summary>
		/// Does a game build
		/// </summary>
		/// <param name="buildFolder"></param>
		/// <param name="folderName"></param>
		/// <param name="projectName"></param>
		/// <param name="scriptsOnly"></param>
		private void DoGameBuild(string buildFolder, string folderName, string projectName, bool scriptsOnly)
		{
			Debug.Log($"Building game to `{buildFolder}{folderName}`...");

			//Configure stuff first, such as scenes and build options
			List<string> levels = ConfigManager.Config.Scenes.Select(scene => scene.SceneLocation).ToList();
			bool isDevBuild = false;
			bool copyPdbFiles = false;
			bool serverBuild = false;
			bool zipFiles = false;

			//If our config manager is using the default build config, then setup these variables
			if (ConfigManager.GetBuildConfig(out DefaultBuildConfig config))
			{
				isDevBuild = config.DevBuild;
				copyPdbFiles = config.CopyPDBFiles;
				serverBuild = config.ServerBuild;
				zipFiles = config.ZipFiles;
			}

			//Setup build options
			BuildOptions buildOptions = BuildOptions.None;
			if (isDevBuild)
				buildOptions |= BuildOptions.Development;
			if (serverBuild)
				buildOptions |= BuildOptions.EnableHeadlessMode;
			if (scriptsOnly)
				buildOptions |= BuildOptions.BuildScriptsOnly;

			//Change Copy PDB files on build setting
			EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", copyPdbFiles ? "true" : "false");

			//We do the build
			BuildReport result = BuildGame(levels.ToArray(), buildFolder + folderName, projectName,
				BuildTarget.StandaloneWindows64, buildOptions);

			//Make sure the build didn't fail
			if (result.summary.result == BuildResult.Failed)
			{
				Debug.LogError("BUILD FAILED!");
				return;
			}

			Debug.Log("Build was a success!");

			if (ConfigManager.GetBuildConfig(out DefaultBuildConfig g))
			{
				Debug.Log("Copying files...");

				foreach (FileToCopy file in g.FilesToCopyOnBuild)
				{
					if (!File.Exists(file.WhatFileToCopy)) continue;

					string fileDir = Path.GetDirectoryName($"{buildFolder}{folderName}/{file.CopyToWhere}");

					if (!Directory.Exists(fileDir))
						Directory.CreateDirectory(fileDir);

					File.Copy(file.WhatFileToCopy, $"{buildFolder}{folderName}/{file.CopyToWhere}", true);
				}
			}

			//Zip files (if enabled)
			if (zipFiles)
			{
				Debug.Log("Zipping build...");
				IZip zip = new DefaultZip();
				zip.CompressDir(buildFolder + folderName, buildFolder + folderName.Replace("/", "") + ".zip");
				Debug.Log("Done!");
			}
		}

		/// <summary>
		/// Does a complete new game build, to a new folder
		/// </summary>
		/// <param name="buildFolder"></param>
		/// <param name="projectName"></param>
		private void FullNewBuild(string buildFolder, string projectName)
		{
			string buildFolderName = $"{projectName}-{DateTime.Now:yy-MM-dd}";
			int count = 0;
			bool folderExists = true;

			//First, we need to find a new folder to build to, since well... its a full new build.
			//I typically name a complete new build by the date, but what if we done a build already today?
			//Well then I just do [Date]-[Count], so add 1, 2, 3 etc to end of the folder name
			//This finds the next available count 
			while (folderExists)
			{
				//First check
				if (count == 0)
				{
					if (Directory.Exists($"{buildFolder}{buildFolderName}/"))
					{
						Debug.Log("Build for today already exists!");

						//The directory already exists, we have a build done already today, so find the next count available
						count++;
					}
					else
					{
						//No build for today, we can just use the date as the name
						folderExists = false;
						continue;
					}
				}

				//The count already exist for today
				if (Directory.Exists($"{buildFolder}{buildFolderName}-{count}/"))
					count++;
				else
				{
					buildFolderName += $"-{count}";
					folderExists = false;
				}
			}

			//Pre create the directory
			Directory.CreateDirectory($"{buildFolder}{buildFolderName}/");

			//Carry on with a new build, except to our new, empty folder
			DoGameBuild(buildFolder, $"{buildFolderName}/", projectName, false);
		}

		#endregion
	}
}