// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Input;
using Team_Capture.SceneManagement;
using UnityCommandLineParser;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Scripting;
using UnityEngine.Video;

namespace Team_Capture.StartUpVideo
{
    /// <summary>
    ///     Controller for the start up video
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(Camera))]
    internal class StartUpVideo : MonoBehaviour
    {
        private static bool skipVideo;

        /// <summary>
        ///     Video clip to play
        /// </summary>
        [Tooltip("Video clip to play")] public AssetReference startUpVideoClip;

        /// <summary>
        ///     The next <see cref="TCScene" /> to load
        /// </summary>
        public AssetReference nextScene;

        private Camera mainCamera;
        private TCScene scene;
        private VideoPlayer startUpVideo;
        private VideoClip videoClip;

        private void Start()
        {
            scene = nextScene.LoadAssetAsync<TCScene>().WaitForCompletion();

            if (skipVideo)
            {
                ChangeScene();
                return;
            }

            videoClip = startUpVideoClip.LoadAssetAsync<VideoClip>().WaitForCompletion();

            InputReader.StartVideoSkip += SkipStartVideo;
            InputReader.EnableStartVideoInput();
            Play();
        }

        private void OnDestroy()
        {
            scene = null;
        }

        [CommandLineCommand("novid")]
        [Preserve]
        public static void NoVid()
        {
            skipVideo = true;
        }

        private void SkipStartVideo()
        {
            InputReader.DisableStartVideoInput();
            InputReader.StartVideoSkip -= SkipStartVideo;

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