using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.AI;

// ReSharper disable BuiltInTypeReferenceStyle

namespace LagCompensation
{
    /// <summary>
    ///     Keeps a record of what position a player was in at what time
    /// </summary>
    public class PlayerPositionRecord
    {
        private const int TicksPerSecond = 20;
        
        /// <summary>
        ///     Holds our <see cref="PlayerPositionRecord" />s, indexed by (server) time
        /// </summary>
        private static Dictionary<float, PlayerPositionRecord[]> playerPositionRecords =
            new Dictionary<float, PlayerPositionRecord[]>();

        private PlayerManager player;
        private Vector3 playerPosition;

        //Called every physics/movement tick
        internal static void DoSeverTick()
        {
            float timeNow = RoundedTickTime(Time.time);

            //Get a list of all our players
            PlayerManager[] players = GameManager.GetAllPlayers();

            PlayerPositionRecord[] positionRecords = new PlayerPositionRecord[players.Length];

            //Get all our records
            for (int i = 0; i < players.Length; i++)
            {
                PlayerManager player = players[i];
                positionRecords[i] = new PlayerPositionRecord
                {
                    player = player,
                    playerPosition = GetPlayerPositionFromIdentifier(player)
                };
            }
            
            //Add our records to the dictionary
            playerPositionRecords.Add(timeNow, positionRecords);
        }

        private static float RoundedTickTime(float t)
        {
            //Rounds to the nearest tick. e.g. if TicksPerSecond is 5, t will be rounded to the nearest 1/5 (0.2s)
            return Mathf.Round(t * TicksPerSecond) / TicksPerSecond;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static Vector3 GetPlayerPositionFromIdentifier(PlayerManager playerManager) => playerManager.transform.position;

        private static bool PlayerWasHit(PlayerManager player, Ray bulletPath, float time)
        {
            //Move the players into the positions they were in at the time of the shot
            for (int i = 0; i < playerPositionRecords.Count; i++)
            {
                playerPositionRecords[time][i].player.transform.position = playerPositionRecords[time][i].playerPosition;
            }
            
            //Do a raycast to see if it hit
            bool hit = Physics.Raycast(bulletPath, out RaycastHit raycastHit, 1000f);

            if (hit)
            {
                Dance()
            }
        }
    }
}