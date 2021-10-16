// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Team_Capture.Helper;

namespace Team_Capture.Editor.Tests
{
    public class DeathScreenCountdownTest
    {
        [Test]
        public async Task CountDownTest()
        {
            const int time = 5;
            int currentTime = 0;

            await TimeHelper.CountDown(time, () =>
            {
                currentTime++;
            });
            
            Assert.AreEqual(time, currentTime);
        }
    }
}