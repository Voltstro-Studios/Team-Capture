using Cysharp.Threading.Tasks;
using Team_Capture.Input;
using Team_Capture.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Video;
using Voltstro.CommandLineParser;

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

		[CommandLineArgument("novid")] public static bool SkipVideo = false;

		/// <summary>
		///		Handles reading inputs
		/// </summary>
		public InputReader inputReader;

		/// <summary>
		///     Video clip to play
		/// </summary>
		[Tooltip("Video clip to play")] public AssetReference startUpVideoClip;

		public TCScene nextScene;

		private void Start()
		{
			if (SkipVideo)
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

		public async void Play()
		{
			await Setup();
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

		public async UniTask Setup()
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
				startUpVideo.clip = await startUpVideoClip.LoadAssetAsync<VideoClip>();
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