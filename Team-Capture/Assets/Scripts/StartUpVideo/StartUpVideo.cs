// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Input;
using Team_Capture.SceneManagement;
using UnityEngine;
using UnityEngine.Video;
using UnityCommandLineParser;
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
		private VideoPlayer startUpVideo;

		private Camera mainCamera;

		[CommandLineCommand("novid")]
		[Preserve]
		public static void NoVid()
		{
			skipVideo = true;
		}
		
		private static bool skipVideo;

		/// <summary>
		///		Handles reading inputs
		/// </summary>
		public InputReader inputReader;

		/// <summary>
		///     Video clip to play
		/// </summary>
		[Tooltip("Video clip to play")] public VideoClip startUpVideoClip;

		public TCScene nextScene;

		private void Start()
		{
			if (skipVideo)
			{
				ChangeScene();
				return;
			}

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
			ChangeScene();
		}

		private void ChangeScene()
		{
			TCScenesManager.LoadScene(nextScene);
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
				startUpVideo.clip = startUpVideoClip;
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