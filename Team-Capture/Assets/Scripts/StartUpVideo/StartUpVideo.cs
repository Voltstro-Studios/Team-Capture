using SceneManagement;
using UnityEngine;
using UnityEngine.Video;

namespace StartUpVideo
{
	/// <summary>
	/// Controller for the start up video
	/// </summary>
	[RequireComponent(typeof(VideoPlayer))]
	[RequireComponent(typeof(Camera))]
	public class StartUpVideo : MonoBehaviour {

		private VideoPlayer startUpVideo;

		private Camera mainCamera;

		/// <summary>
		/// Can we skip the start up video?
		/// </summary>
		[Tooltip("Can we skip the start up video?")]
		public bool canSkip = true;

		/// <summary>
		/// What key do we use to skip the start up video
		/// </summary>
		[Tooltip("What key do we use to skip the start up video")]
		public KeyCode skipKey = KeyCode.Escape;
    
		/// <summary>
		/// Video clip to play
		/// </summary>
		[Tooltip("Video clip to play")]
		public VideoClip startUpVideoClip;

		private bool inPlayMode;

#if TEAM_CAPTURE
		public TCScene nextScene;
#else
		public string nextSceneName = "MainMenu";
#endif

		private void Start ()
		{
			inPlayMode = true;
			Play();
		}

		public void Play()
		{
			Setup();
			startUpVideo.Play();
		}

		private void Update()
		{
			if (!inPlayMode)
				return;

			if (!canSkip) return;

			if (!Input.GetKeyDown(skipKey)) return;

			startUpVideo.Pause();
			ChangeScene();
		}

		private void VideoEnd(VideoPlayer source)
		{
			ChangeScene();
		}

		private void ChangeScene()
		{
#if TEAM_CAPTURE
			TCScenesManager.LoadScene(nextScene);
#else
			SceneManager.LoadScene(nextSceneName);
#endif
		}

		public void Setup()
		{
			startUpVideo = GetComponent<VideoPlayer>();
			mainCamera = GetComponent<Camera>();
			startUpVideo.loopPointReached += VideoEnd;

			//Setup Video player
			if (startUpVideo != null)
			{
				if(startUpVideoClip == null)
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