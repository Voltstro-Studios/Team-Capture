using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

// ReSharper disable BuiltInTypeReferenceStyle

namespace LagCompensation
{
    /// <summary>
    ///     Keeps a record of what position a player was in at what time
    /// </summary>
    public class PlayerPositionRecord
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
        }

        private const int TicksPerSecond = 20;

        /// <summary>
        ///     Holds our <see cref="PlayerPositionRecord" />s, indexed by (server) time
        /// </summary>
        private static Dictionary<float, PlayerPositionRecord[]> playerPositionRecords =
            new Dictionary<float, PlayerPositionRecord[]>();

        private PlayerManager playerManager;
        private Vector3 playerPosition;

        private static float TimeNow => RoundedTickTime(Time.time);

        //Called every physics/movement tick
        internal static void RecordPlayerPositions()
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
                    playerManager = player,
                    playerPosition = GetPlayerPositionFromIdentifier(player)
                };
            }

            //Add our records to the dictionary
            //TODO: Make old entries automatically get removed after a certain time (maybe 3sec?)
            if (!playerPositionRecords.ContainsKey(TimeNow))
                playerPositionRecords.Add(TimeNow, positionRecords);
            else
                playerPositionRecords[TimeNow] = positionRecords;
        }

        private static float RoundedTickTime(float t)
        {
            //Rounds to the nearest tick. e.g. if TicksPerSecond is 5, t will be rounded to the nearest 1/5 (0.2s)
            return Mathf.Round(t * TicksPerSecond) / TicksPerSecond;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static Vector3 GetPlayerPositionFromIdentifier(PlayerManager playerManager) =>
            playerManager.transform.position;

        public static void Simulate(float time, Action action)
        {
            //Round our time to ensure we don't get invalid index for our dictionary
            time = RoundedTickTime(time);
            //Update our player positions just in case
            RecordPlayerPositions();

            //Move the players into the positions they were in at the time of the action
            for (int i = 0; i < playerPositionRecords.Count; i++)
            {
                //Move all our players
                playerPositionRecords[TimeNow][i].playerManager.transform.position =
                    //Into the position they were in at the time of simulation
                    playerPositionRecords[time][i].playerPosition;
            }

            //Simulate the action
            action();

            //Now move all our players back to where they are now
            for (int i = 0; i < playerPositionRecords.Count; i++)
            {
                //Move all the players back to where they were before simulation
                playerPositionRecords[TimeNow][i].playerManager.transform.position =
                    playerPositionRecords[TimeNow][i].playerPosition;
            }
        }
    }
}