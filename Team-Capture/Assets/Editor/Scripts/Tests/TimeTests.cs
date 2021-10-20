// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using NUnit.Framework;
using Team_Capture.Helper;
using UnityEngine;

namespace Team_Capture.Editor.Tests
{
    public class TimeTests
    {
        [Test]
        public void CountDownTest()
        {
            const int time = 5;

            TimeHelper.CountDown(time, tick => Debug.Log($"Tick: {tick}"));
        }
    }
}