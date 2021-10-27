﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using Team_Capture.Core.Networking;
using Team_Capture.Player;
using UnityEngine;

namespace Team_Capture.LagCompensation
{
    internal static class SimulationHelper
    {
        public static readonly List<SimulationObject> SimulationObjects = new();
        public static int CurrentFrame { get; private set; }

        //Every time this is called, tell all our simulation objects to add a frame
        public static void UpdateSimulationObjectData()
        {
            CurrentFrame++;
            for (int i = 0; i < SimulationObjects.Count; i++) SimulationObjects[i].AddFrame();
        }

        public static void SimulateCommand(PlayerManager playerExecutedCommand, Action command)
        {
            int playersLatency =
                (int) PingManager.GetClientPing(playerExecutedCommand.netIdentity.connectionToClient.connectionId);

            //Logger.Log($"Player's ping is {playersLatency}", LogVerbosity.Debug);

            int frameId = CurrentFrame - playersLatency;
            //Debug.Log($"Current frame is {CurrentFrame}, using frame {frameId}");

            //if (frameId > TCNetworkManager.Instance.maxFrameCount)
            //	frameId = TCNetworkManager.Instance.maxFrameCount;

            Simulate<object>(frameId, () =>
            {
                command();
                return null;
            });
        }

        /// <summary>
        ///     Simulates an action at a previous point in time, with each <see cref="SimulationObjects" />'s
        ///     <see cref="Transform" /> changed back as it was
        /// </summary>
        /// <param name="frameId">
        ///     The frame at which to simulate. If negative or zero, will be how many frames to go back from the
        ///     current one
        /// </param>
        /// <param name="function">The <see cref="Func{T}" /> to run. The value returned by the function is returned</param>
        /// <param name="clientSubFrameLerp">
        ///     An optional modifier to change how much the position and rotation are interpolated
        ///     with the next frame
        /// </param>
        /// <returns>The value returned by the <paramref name="function" /></returns>
        /// <typeparam name="T">
        ///     A generic type to pass to the <paramref name="function" /> as a return value. An object of this
        ///     type is returned.
        /// </typeparam>
        /// <returns></returns>
        public static T Simulate<T>(int frameId, Func<T> function, float clientSubFrameLerp = 0)
        {
            //This is a check to ensure that we don't try to lerp from the current frame into the future one that hasn't been created yet
            if (frameId == CurrentFrame)
                frameId--;

            for (int i = 0; i < SimulationObjects.Count; i++)
                SimulationObjects[i].SetStateTransform(frameId, clientSubFrameLerp);

            T result = function.Invoke();

            for (int i = 0; i < SimulationObjects.Count; i++) SimulationObjects[i].ResetStateTransform();

            return result;
        }
    }
}