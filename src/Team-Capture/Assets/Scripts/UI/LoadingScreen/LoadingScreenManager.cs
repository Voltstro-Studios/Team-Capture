// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections;
using Mirror;
using Team_Capture.Core;
using Team_Capture.SceneManagement;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.UI.LoadingScreen
{
	/// <summary>
	///     Handles when to create <see cref="LoadingScreenUI" /> and manages it.
	/// </summary>
	internal class LoadingScreenManager : SingletonMonoBehaviour<LoadingScreenManager>
    {
	    /// <summary>
	    ///     Are we loading?
	    /// </summary>
	    public static bool IsLoading;

	    /// <summary>
	    ///     Is the <see cref="LoadingScreenManager" /> setup?
	    /// </summary>
	    private static bool isSetup;

        [SerializeField] private GameObject loadingScenePrefab;

        protected override void SingletonStarted()
        {
            //Make sure loadingScenePrefab isn't null
            if (loadingScenePrefab == null)
            {
                Logger.Error("LoadingScenePrefab is null!");
                Destroy(gameObject);
                return;
            }

            MirrorSceneManager.OnBeginSceneLoading += OnBeginSceneLoading;
            isSetup = true;
        }

        protected override void SingletonDestroyed()
        {
            if (isSetup)
                MirrorSceneManager.OnBeginSceneLoading -= OnBeginSceneLoading;
        }

        private void OnBeginSceneLoading(AsyncOperation operation, string sceneName)
        {
            StartCoroutine(OnStartSceneLoadAsync(operation, TCScenesManager.FindSceneInfo(sceneName)));
        }

        private IEnumerator OnStartSceneLoadAsync(AsyncOperation sceneLoadOperation, TCScene scene)
        {
            //Check to make sure that we aren't already loading, that LoadingScreenManager is setup, and that we aren't quitting
            if (sceneLoadOperation == null || IsLoading || !isSetup || Game.IsGameQuitting || scene == null)
                yield break;

            IsLoading = true;

            //Create LoadingScreenUI and set it up
            LoadingScreenUI loadingScreenUI =
                Instantiate(loadingScenePrefab).GetComponent<LoadingScreenUI>();
            loadingScreenUI.Setup(scene);

            //While we are loading, set the progress bar to sceneLoadOperation progress
            // ReSharper disable once PossibleNullReferenceException
            while (!sceneLoadOperation.isDone)
            {
                loadingScreenUI.SetLoadingBarAmount(Mathf.Clamp01(sceneLoadOperation.progress / .9f));

                yield return null;
            }

            //We are done, no need to destroy loadingScreenUI as it will automatically be done by changing scenes
            IsLoading = false;
        }
    }
}