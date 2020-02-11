using System;
using System.Collections.Generic;
using Core;
using Player;
using UnityEngine;

namespace LagCompensation
{
	public class SimulationHelper
	{
		public static readonly List<SimulationObject> SimulationObjects = new List<SimulationObject>();
		public static int CurrentFrame { get; private set; }

		//Every time this is called, tell all our simulation objects to add a frame
		public static void UpdateSimulationObjectData()
		{
			CurrentFrame++;
			for (int i = 0; i < SimulationObjects.Count; i++) SimulationObjects[i].AddFrame();
		}

		public static void SimulateCommand(PlayerManager playerExecutedCommand, Action command)
		{
			//TODO: Figure out what frame ID to use
			int frameId = 1;

			if (frameId > TCNetworkManager.Instance.maxFrameCount)
				frameId = TCNetworkManager.Instance.maxFrameCount;

			Simulate(frameId, command, playerExecutedCommand);
		}

		/// <summary>
		/// Simulates an action at a previous point in time, with each <see cref="SimulationObjects"/>'s
		/// <see cref="Transform"/> changed back as it was
		/// </summary>
		/// <param name="frameId">The frame at which to simulate</param>
		/// <param name="function">The <see cref="Func{T}"/> to run. The value returned by the function is returned</param>
		/// <param name="playerWhoRanSimulate"></param>
		/// <param name="clientSubFrameLerp">
		/// An optional modifier to change how much the position and rotation are interpolated
		/// with the next frame
		/// </param>
		/// <returns>The value returned by the <paramref name="function"/></returns>
		public static void Simulate(int frameId, Action function, PlayerManager playerWhoRanSimulate,
			float clientSubFrameLerp = 0)
		{
			//TODO: No clue why this is there, needs checking if it's useful at all
			if (frameId == CurrentFrame)
				frameId--;

			foreach (SimulationObject simulatedObject in SimulationObjects)
				simulatedObject.SetStateTransform(frameId, clientSubFrameLerp);

			function();

			foreach (SimulationObject simulatedObject in SimulationObjects) simulatedObject.ResetStateTransform();
		}
	}
}