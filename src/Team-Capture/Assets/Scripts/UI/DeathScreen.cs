// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Threading;
using Cysharp.Threading.Tasks;
using Team_Capture.Helper;
using Team_Capture.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Team_Capture.UI
{
    internal class DeathScreen : MonoBehaviour
    {
        [SerializeField] private GameObject playerDeathPrefab;
        [SerializeField] private GameObject panelsObject;
        [SerializeField] private TMP_Text countDownText;
        [SerializeField] private GameObject killedByPanel;
        [SerializeField] private TMP_Text killedByText;
        [SerializeField] private TMP_Text killedByHealthText;
        [SerializeField] private RawImage killedByProfileImage;
        [SerializeField] private int playerDeathCamTime = 10;

        private CancellationTokenSource cancellationTokenSource;
        private PlayerDeathCam playerDeathCam;

        private PlayerManager playerManager;

        private int stopDeathCamTime;

        private void OnDisable()
        {
            cancellationTokenSource.Cancel();
            panelsObject.SetActive(false);
        }

        internal void Setup(PlayerManager setPlayerManager)
        {
            playerManager = setPlayerManager;
            playerDeathCam = Instantiate(playerDeathPrefab).GetComponent<PlayerDeathCam>();
            playerDeathCam.gameObject.SetActive(false);
        }

        public void StartCountDown(PlayerManager killer, int time)
        {
            if (killer == null)
                return;

            stopDeathCamTime = time - playerDeathCamTime;
            countDownText.text = time.ToString();

            GameObject deathCamObject = playerDeathCam.gameObject;
            Transform deathCamTransform = deathCamObject.transform;
            Transform playerTransform = playerManager.transform;
            deathCamTransform.position = playerTransform.position;
            deathCamTransform.rotation = playerTransform.rotation;
            deathCamObject.SetActive(true);

            killedByPanel.SetActive(true);
            killedByText.text = killer.User.UserName;
            killedByHealthText.text = $"{killer.Health.ToString()} HP Left.";
            killedByProfileImage.texture = playerManager.User.UserProfilePicture;


            if (killer != playerManager)
                playerDeathCam.StartTrackingPlayer(killer);

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            cancellationTokenSource = new CancellationTokenSource();

            TimeHelper.CountDown(time, OnCountdownTick, cancellationTokenSource.Token).ContinueWith(
                () =>
                {
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                });
        }

        private void OnCountdownTick(int tick)
        {
            if (tick == stopDeathCamTime)
            {
                playerDeathCam.StopTrackingPlayer();
                playerDeathCam.gameObject.SetActive(false);
                panelsObject.SetActive(true);
                killedByPanel.SetActive(false);
            }

            countDownText.text = tick.ToString();
        }
    }
}