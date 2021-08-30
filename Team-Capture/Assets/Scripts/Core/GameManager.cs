// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Player;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Core
{
	public class GameManager : SingletonMonoBehaviour<GameManager>
	{
		protected override bool DoDestroyOnLoad => true;

		protected override void SingletonDestroyed()
		{
			ClearAllPlayers();
		}

		#region Player Tracking

		private const string PlayerIdPrefix = "Player ";

		private static readonly Dictionary<string, PlayerManager> Players = new Dictionary<string, PlayerManager>();

		public delegate void PlayerEvent(string playerId);

		public static event PlayerEvent PlayerAdded;
		public static event PlayerEvent PlayerRemoved;

		/// <summary>
		///     Adds a <see cref="PlayerManager" />
		/// </summary>
		/// <param name="netId"></param>
		/// <param name="playerManager"></param>
		public static void AddPlayer(string netId, PlayerManager playerManager)
		{
			string playerId = PlayerIdPrefix + netId;
			playerManager.gameObject.name = playerId;
			Players.Add(playerId, playerManager);

			PlayerAdded?.Invoke(playerId);
			Logger.Debug("Added player {@PlayerId}.", playerId);
		}

		/// <summary>
		///     Removes a <see cref="PlayerManager" /> using their assigned ID
		/// </summary>
		/// <param name="playerId"></param>
		public static void RemovePlayer(string playerId)
		{
			Players.Remove(playerId);

			PlayerRemoved?.Invoke(playerId);
			Logger.Debug("Removed player {@PlayerId}", playerId);
		}

		/// <summary>
		///     Returns a <see cref="PlayerManager" /> using their assigned ID
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		public static PlayerManager GetPlayer(string playerId)
		{
			return Players[playerId];
		}

		/// <summary>
		///     Gets all <see cref="PlayerManager" />s
		/// </summary>
		/// <returns></returns>
		public static PlayerManager[] GetAllPlayers()
		{
			return Players.Values.ToArray();
		}

		/// <summary>
		///     Clears all players from the players list
		/// </summary>
		public static void ClearAllPlayers()
		{
			Players.Clear();
		}

		#endregion

		#region Commands

		[ConCommand("sv_damage", "Damages a player", CommandRunPermission.ServerOnly, 2, 2)]
		public static void DamageCommandCommand(string[] args)
		{
			if (Instance == null)
			{
				Logger.Error("A game isn't currently running!");
				return;
			}

			if (NetworkManager.singleton.mode != NetworkManagerMode.ServerOnly)
			{
				Logger.Error("You can only run this command on a server!");
				return;
			}

			string playerId = args[0];
			PlayerManager player = GetPlayer(playerId);
			if (player == null)
			{
				Logger.Error("A player with that ID doesn't exist!");
				return;
			}

			string damage = args[1];
			if (int.TryParse(damage, out int result))
				player.TakeDamage(result, playerId);
			else
				Logger.Error("The imputed damage to do isn't a number!");
		}

		[ConCommand("players", "Gets a list of all the players")]
		public static void ListPlayersCommand(string[] args)
		{
			if (Instance == null)
			{
				Logger.Error("A game isn't currently running!");
				return;
			}

			Logger.Info("== Connected Players ==");
			foreach (PlayerManager playerManager in GetAllPlayers())
				Logger.Info(" Name: {@PlayerName} - ID: {@PlayerNetID}", playerManager.User.UserName, playerManager.netId);
		}

		#endregion
	}
}