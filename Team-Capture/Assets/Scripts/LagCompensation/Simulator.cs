using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace LagCompensation
{
	internal static partial class Simulator
	{
		private const int TicksPerSecond = 20;

		/// <summary>
		/// Holds our <see cref="PlayerPositionRecord"/>s, indexed by (server) time
		/// </summary>
		private static readonly Dictionary<float, PlayerPositionRecord[]> PlayerPositionRecords =
			new Dictionary<float, PlayerPositionRecord[]>();

		private static float RoundedTimeNow => RoundedTickTime(Time.time);

		//Should be called every tick
		private static void RecordPlayerPositions()
		{
			//Get a list of all our players
			PlayerManager[] players = GameManager.GetAllPlayers();

			PlayerPositionRecord[] positionRecords = new PlayerPositionRecord[players.Length];

			//Get all our records
			for (int i = 0; i < players.Length; i++)
			{
				PlayerManager player = players[i];
				positionRecords[i] = new PlayerPositionRecord
				{
					PlayerManager = player,
					PlayerPosition = GetPlayerPositionFromIdentifier(player)
				};
			}

			//Add our records to the dictionary
			//TODO: Make old entries automatically get removed after a certain time (maybe 3sec?)
			if (!PlayerPositionRecords.ContainsKey(RoundedTimeNow))
				PlayerPositionRecords.Add(RoundedTimeNow, positionRecords);
			else
				PlayerPositionRecords[RoundedTimeNow] = positionRecords;
		}

		private static float RoundedTickTime(float t)
		{
			//Rounds to the nearest tick. e.g. if TicksPerSecond is 5, t will be rounded to the nearest 1/5 (0.2s)
			return Mathf.Round(t * TicksPerSecond) / TicksPerSecond;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private static Vector3 GetPlayerPositionFromIdentifier(PlayerManager playerManager)
		{
			return playerManager.transform.position;
		}

		public static void Simulate(float time, Action action)
		{
			//Round our time to ensure we don't get invalid index for our dictionary
			time = RoundedTickTime(time);
			//Update our player positions just in case
			RecordPlayerPositions();

			//Move the players into the positions they were in at the time of the action
			for (int i = 0; i < PlayerPositionRecords.Count; i++)
				//Move all our players
				PlayerPositionRecords[RoundedTimeNow][i].PlayerManager.transform.position =
					//Into the position they were in at the time of simulation
					PlayerPositionRecords[time][i].PlayerPosition;

			//Simulate the action
			action();

			//Now move all our players back to where they are now
			for (int i = 0; i < PlayerPositionRecords.Count; i++)
				//Move all the players back to where they were before simulation
				PlayerPositionRecords[RoundedTimeNow][i].PlayerManager.transform.position =
					PlayerPositionRecords[RoundedTimeNow][i].PlayerPosition;
		}
	}
}