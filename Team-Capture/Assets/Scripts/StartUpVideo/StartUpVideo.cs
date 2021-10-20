// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using Team_Capture.Input;
using Team_Capture.SceneManagement;
using UnityEngine;
using UnityEngine.Video;
using UnityCommandLineParser;
using UnityEngine.AddressableAssets;
using UnityEngine.Scripting;

namespace Team_Capture.StartUpVideo
{
	/// <summary>
	///     Controller for the start up video
	/// </summary>
	[RequireComponent(typeof(VideoPlayer))]
	[RequireComponent(typeof(Camera))]
	internal class StartUpVideo : MonoBehaviour
	{
		private VideoClip videoClip;
		private VideoPlayer startUpVideo;
		private TCScene scene;
		private Camera mainCamera;
		private static bool skipVideo;

		[CommandLineCommand("novid")]
		[Preserve]
		public static void NoVid()
		{
			skipVideo = true;
		}

		/// <summary>
		///		Handles reading inputs
		/// </summary>
		public InputReader inputReader;

		/// <summary>
		///     Video clip to play
		/// </summary>
		[Tooltip("Video clip to play")] 
		public AssetReference startUpVideoClip;

		/// <summary>
		///		The next <see cref="TCScene"/> to load
		/// </summary>
		public AssetReference nextScene;

		private void Start()
		{
			scene = nextScene.LoadAssetAsync<TCScene>().WaitForCompletion();
			
			if (skipVideo)
			{
				ChangeScene();
				return;
			}

			videoClip = startUpVideoClip.LoadAssetAsync<VideoClip>().WaitForCompletion();
			
			inputReader.StartVideoSkip += SkipStartVideo;
			inputReader.EnableStartVideoInput();
			Play();
		}

		private void SkipStartVideo()
		{
			inputReader.DisableStartVideoInput();
			inputReader.StartVideoSkip -= SkipStartVideo;

			startUpVideo.Pause();
			ChangeScene();
		}

		public void Play()
		{
			Setup();
			startUpVideo.Play();
		}

		private void VideoEnd(VideoPlayer source)
		{
			startUpVideo.clip = null;
			Destroy(videoClip);
			ChangeScene();
		}

		private void ChangeScene()
		{
			TCScenesManager.LoadScene(scene);
		}

		private void OnDestroy()
		{
			scene = null;
		}

		public void Setup()
		{
			startUpVideo = GetComponent<VideoPlayer>();
			mainCamera = GetComponent<Camera>();
			startUpVideo.loopPointReached += VideoEnd;

			//Setup Video player
			if (startUpVideo != null)
			{
				if (startUpVideoClip == null)
				{
					Debug.LogError("You need to reference a video!");
					return;
				}

				startUpVideo.source = VideoSource.VideoClip;
				startUpVideo.clip = videoClip;
				startUpVideo.renderMode = VideoRenderMode.CameraNearPlane;
				startUpVideo.playOnAwake = false;
				startUpVideo.waitForFirstFrame = true;
				startUpVideo.isLooping = false;
				startUpVideo.targetCamera = mainCamera;
			}

			//Setup camera
			if (mainCamera == null) return;

			mainCamera.tag = "MainCamera";
			mainCamera.fieldOfView = 60;
			mainCamera.allowHDR = true;
			mainCamera.clearFlags = CameraClearFlags.SolidColor;
			mainCamera.backgroundColor = Color.black;
		}
	}
}