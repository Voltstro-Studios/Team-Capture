// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Threading;
using Cysharp.Threading.Tasks;
using Team_Capture.Helper;
using TMPro;
using UnityEngine;

namespace Team_Capture.UI
{
    internal class DeathScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text countDownText;
        private CancellationTokenSource cancellationTokenSource;

        public void StartCountDown(int time)
        {
            countDownText.text = time.ToString();
            
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            
            cancellationTokenSource = new CancellationTokenSource();

            TimeHelper.CountDown(time, tick => countDownText.text = tick.ToString(), cancellationTokenSource.Token).ContinueWith(
                () =>
                {
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                });
        }

        private void OnDisable()
        {
            cancellationTokenSource.Cancel();
        }
    }
}