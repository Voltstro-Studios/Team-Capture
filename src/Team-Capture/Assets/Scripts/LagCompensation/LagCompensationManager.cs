// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using Team_Capture.Core.Networking;
using Team_Capture.Logging;
using Team_Capture.Player;
using UnityEngine.Scripting;

//This lag compensator was originally written by @EternalClickbait, based off theses:
// - https://twoten.dev/lag-compensation-in-unity.html
// - https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking#Lag_compensation
//               (The usual article everyone refers to when anything lag compensation related)
//
//But he kinda did it half-assed, and then Voltstro kinda half-assed the rest of it.
//Plus we were missing our current PingManager at the time, meaning we had no way of getting the latency of a client.
//Note: Mirror still (at the time of writting this) doesn't fucking have a way of getting the latency of a client. Their are even community provided ways in their Discord ffs.
//
//It was then re-done based off https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/da6250d3a96c0344c03e5a490897dcc298e77e2e/com.community.netcode.extensions/Runtime/LagCompensation
//While the one in Unity's community repo is the original from twoten's article it was "modified to be used with latency rather than fixed frames and subframes".

namespace Team_Capture.LagCompensation
{
    /// <summary>
    ///     Handles lag compensation
    /// </summary>
    [Preserve]
    public static class LagCompensationManager
    {
        private static List<LagCompensatedObject> lagCompensatedObjects;

        /// <summary>
        ///     Sets up the server side of <see cref="LagCompensationManager" />
        /// </summary>
        internal static void ServerSetup()
        {
            lagCompensatedObjects = new List<LagCompensatedObject>();
        }

        /// <summary>
        ///     Shuts down the server side of <see cref="LagCompensationManager" />
        /// </summary>
        internal static void ServerShutdown()
        {
            lagCompensatedObjects.Clear();
            lagCompensatedObjects = null;
        }

        /// <summary>
        ///     Add a <see cref="LagCompensatedObject" /> to this manager
        /// </summary>
        /// <param name="obj"></param>
        internal static void AddLagCompensatedObject(LagCompensatedObject obj)
        {
            lagCompensatedObjects.Add(obj);
        }

        /// <summary>
        ///     Removes a <see cref="LagCompensatedObject" /> from this manager
        /// </summary>
        /// <param name="obj"></param>
        internal static void RemoveLagCompensatedObject(LagCompensatedObject obj)
        {
            lagCompensatedObjects.Remove(obj);
        }

        /// <summary>
        ///     Adds a frame to every lag compensated object
        /// </summary>
        internal static void ServerUpdate()
        {
            for (int i = 0; i < lagCompensatedObjects.Count; i++)
                lagCompensatedObjects[i].AddFrame();
        }

        /// <summary>
        ///     Simulates an <see cref="Action" /> for a <see cref="PlayerManager" />
        /// </summary>
        /// <param name="playerExecutedCommand"></param>
        /// <param name="command"></param>
        public static void Simulate(PlayerManager playerExecutedCommand, Action command)
        {
            double playerLatency = PingManager.GetClientPing(playerExecutedCommand.connectionToClient.connectionId);

            Simulate(playerLatency, command);
        }

        /// <summary>
        ///     Simulates an <see cref="Action" />
        /// </summary>
        /// <param name="secondsAgo"></param>
        /// <param name="command"></param>
        public static void Simulate(double secondsAgo, Action command)
        {
            for (int i = 0; i < lagCompensatedObjects.Count; i++)
                lagCompensatedObjects[i].SetStateTransform(secondsAgo);

            try
            {
                command();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error handing simulation of command!");
            }

            for (int i = 0; i < lagCompensatedObjects.Count; i++) lagCompensatedObjects[i].ResetStateTransform();
        }
    }
}