// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using Team_Capture.Core.Networking;
using Team_Capture.Player;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.LagCompensation
{
    internal static class SimulationHelper
    {
        public static readonly List<SimulationObject> SimulationObjects = new();

        //Every time this is called, tell all our simulation objects to add a frame
        public static void UpdateSimulationObjectData()
        {
            for (int i = 0; i < SimulationObjects.Count; i++) 
                SimulationObjects[i].AddFrame();
        }

        public static void SimulateCommand(PlayerManager playerExecutedCommand, Action command)
        {
            double playerLatency = PingManager.GetClientPing(playerExecutedCommand.connectionToClient.connectionId);

            for (int i = 0; i < SimulationObjects.Count; i++)
            {
                SimulationObjects[i].SetStateTransform(playerLatency);
            }

            try
            {
                command();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error handing simulation of command!");
            }

            for (int i = 0; i < SimulationObjects.Count; i++)
            {
                SimulationObjects[i].ResetStateTransform();
            }
        }
    }
}