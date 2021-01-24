using System;
using System.Collections;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Core;
using Team_Capture.Core.Networking;
using Team_Capture.Player.Movement;
using Team_Capture.Weapons;
using UnityEngine;
using Voltstro.CommandLineParser;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Player
{
	/// <summary>
	///		The primary <see cref="Behaviour"/> for managing the player.
	///		<para>This class manages health, death, respawning as well some other minor things.</para>
	/// </summary>
	public sealed class PlayerManager : NetworkBehaviour
	{
		/// <summary>
		///     <see cref="GameObject" />s to disable/enable on death and respawn
		/// </summary>
		[SerializeField] private GameObject[] disableGameObjectsOnDeath;

		/// <summary>
		///     How long the invincibility lasts on spawn
		/// </summary>
		[SerializeField] private float invincibilityLastTime = 3.0f;

		/// <summary>
		///     How often to update the latency on the scoreboard
		/// </summary>
		[SerializeField] private float playerLatencyUpdateTime = 2.0f;

		/// <summary>
		///     The max health
		/// </summary>
		public int MaxHealth { get; } = 100;

		#region Movement

		/// <summary>
		///     The player's <see cref="PlayerMovementManager" />
		/// </summary>
		private PlayerMovementManager playerMovementManager;

		/// <summary>
		///     The player's <see cref="CharacterController" />
		/// </summary>
		private CharacterController characterController;

		#endregion

		#region Sync Vars

		/// <summary>
		///     Is this player dead?
		/// </summary>
		[field: SyncVar]
		public bool IsDead { get; private set; } = true;

		/// <summary>
		///     The username of this player
		/// </summary>
		[SyncVar] public string username = "Not Set";

		/// <summary>
		///     How much health does this player have
		/// </summary>
		[field: SyncVar(hook = nameof(OnPlayerDamaged))]
		public int Health { get; private set; }

		/// <summary>
		///     How many kills does this player have
		/// </summary>
		[field: SyncVar(hook = nameof(OnPlayerKilled))]
		public int Kills { get; private set; }

		/// <summary>
		///     How many deaths does this player have
		/// </summary>
		[field: SyncVar(hook = nameof(OnPlayerDeath))]
		public int Deaths { get; private set; }

		/// <summary>
		///     Updated every <see cref="playerLatencyUpdateTime"/> by the server of the player's latency (rtt)
		/// </summary>
		[SyncVar] public double latency;

		#endregion

		#region Event System

		/// <summary>
		///     Invoked when this player takes damage
		/// </summary>
		public event Action PlayerDamaged;

		/// <summary>
		///     Invoked when this player dies
		/// </summary>
		public event Action PlayerDeath;

		/// <summary>
		///     Invoked when this player gets a kill
		/// </summary>
		public event Action PlayerKill;

		// ReSharper disable UnusedParameter.Local
		private void OnPlayerDeath(int oldValue, int newValue)
		{
			PlayerDeath?.Invoke();
		}

		private void OnPlayerKilled(int oldValue, int newValue)
		{
			PlayerKill?.Invoke();
		}

		private void OnPlayerDamaged(int oldHealth, int newHealth)
		{
			PlayerDamaged?.Invoke();
		}
		// ReSharper restore UnusedParameter.Local

		#endregion

		#region Server Variables

		/// <summary>
		///     Only set on the server!
		///     <para>The player's active <see cref="WeaponManager" /></para>
		/// </summary>
		private WeaponManager weaponManager;

		/// <summary>
		///     Only set on the server!
		///     <para>Is this player invincible?</para>
		/// </summary>
		public bool IsInvincible { get; private set; } = true;

		#endregion

		#region Client Variables

		/// <summary>
		///     Manages UI
		/// </summary>
		private PlayerUIManager uiManager;

		#endregion

		#region Unity Methods

		private void Awake()
		{
			playerMovementManager = GetComponent<PlayerMovementManager>();
			characterController = GetComponent<CharacterController>();
		}

		private void Start()
		{
			if (!isLocalPlayer) return;

			uiManager = GetComponent<PlayerUIManager>();
			CmdSetName(StartPlayerName);
		}

		#endregion

		#region Network Methods

		public override void OnStartServer()
		{
			Health = MaxHealth;
			weaponManager = GetComponent<WeaponManager>();

			StartCoroutine(UpdateLatency());
			StartCoroutine(ServerPlayerRespawn(true));
		}

		public override void OnStopServer()
		{
			StopAllCoroutines();
		}

		#endregion

		#region Death, Respawn, Damage

		/// <summary>
		///     Take damage
		/// </summary>
		/// <param name="damageAmount"></param>
		/// <param name="sourcePlayerId"></param>
		[Server]
		public void TakeDamage(int damageAmount, string sourcePlayerId)
		{
			if (string.IsNullOrWhiteSpace(sourcePlayerId))
			{
				Logger.Error("The {@ArgumentName} cannot be empty or null!", nameof(sourcePlayerId));
				return;
			}

			//Can't do damage on a player if they are dead or just re-spawned
			if (IsDead || IsInvincible) return;

			Health -= damageAmount;

			if (Health > 0) return;

			//Player is dead
			ServerPlayerDie(sourcePlayerId);
		}

		/// <summary>
		///     Adds health to the player
		/// </summary>
		/// <param name="amount"></param>
		[Server]
		public void AddHealth(int amount)
		{
			if (Health == amount)
				return;

			if (Health + amount > MaxHealth)
			{
				Health = MaxHealth;
				return;
			}

			Health += amount;
		}

		/// <summary>
		///     The server side method that handles a player's death
		/// </summary>
		[Server]
		private void ServerPlayerDie(string sourcePlayerId)
		{
			IsDead = true;

			//Send a message about this player's death
			NetworkServer.SendToAll(new PlayerDiedMessage
			{
				PlayerKilled = transform.name,
				PlayerKiller = sourcePlayerId,
				WeaponName = weaponManager.GetActiveWeapon().Weapon
			}, 3);

			//Remove all the weapons on the player
			weaponManager.RemoveAllWeapons();

			//Call the client side code on each player
			RpcClientPlayerDie();

			//Disable movement
			playerMovementManager.enabled = false;
			characterController.enabled = false;

			//Update the stats, for both players
			PlayerManager killer = GameManager.GetPlayer(sourcePlayerId);
			Deaths++;
			if (sourcePlayerId != transform.name)
				killer.Kills++;

			StartCoroutine(ServerPlayerRespawn());
		}

		/// <summary>
		///     Server side method that handles the player's
		/// </summary>
		/// <returns></returns>
		[Server]
		internal IEnumerator ServerPlayerRespawn(bool skipRespawnTime = false)
		{
			if (!skipRespawnTime)
				yield return new WaitForSeconds(GameManager.GetActiveScene().respawnTime);

			characterController.enabled = true;
			playerMovementManager.enabled = true;

			Health = MaxHealth;

			RpcClientRespawn();

			//Add stock weapons
			weaponManager.AddStockWeapons();

			//Set position to spawn
			Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
			Quaternion rotation = spawnPoint.rotation;
			playerMovementManager.SetCharacterPosition(spawnPoint.position, rotation.x, rotation.y, true);

			IsDead = false;
			IsInvincible = true;
			yield return new WaitForSeconds(invincibilityLastTime);

			IsInvincible = false;
		}

		/// <summary>
		///     Disables components on each of the clients
		/// </summary>
		[ClientRpc]
		private void RpcClientPlayerDie()
		{
			try
			{
				playerMovementManager.DisableStateHandling();

				//Disable the collider, or the Char controller
				if (isLocalPlayer)
				{
					//Switch cams
					GameManager.GetActiveSceneCamera().SetActive(true);

					//Disable the HUD
					uiManager.SetHud(false);
				}

				//Disable movement
				playerMovementManager.enabled = false;
				characterController.enabled = false;

				foreach (GameObject toDisable in disableGameObjectsOnDeath) toDisable.SetActive(false);
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Something went wrong in {MethodName}!", nameof(RpcClientPlayerDie));
			}
		}

		/// <summary>
		///     Client side method of enabling client side stuff per client
		/// </summary>
		[ClientRpc]
		private void RpcClientRespawn()
		{
			try
			{
				//Enable game objects
				foreach (GameObject toEnable in disableGameObjectsOnDeath) toEnable.SetActive(true);

				//Enable movement
				characterController.enabled = true;
				playerMovementManager.enabled = true;

				//Enable the collider, or the Char controller
				if (isLocalPlayer)
				{
					//Switch cams
					GameManager.GetActiveSceneCamera().SetActive(false);

					//Enable our HUD
					uiManager.SetHud(true);
				}

				playerMovementManager.EnableStateHandling();
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Something went wrong in {MethodName}!", nameof(RpcClientRespawn));
			}
		}

		#endregion

		#region Helper Methods

		/// <summary>
		///     Kills the player
		/// </summary>
		[Command(channel = Channels.DefaultUnreliable)]
		public void CmdSuicide()
		{
			TakeDamage(Health, transform.name);
		}

		private IEnumerator UpdateLatency()
		{
			while (isServer)
			{
				latency = PingManager.GetClientPing(connectionToClient.connectionId);
				yield return new WaitForSeconds(playerLatencyUpdateTime);
			}
		}

		#endregion

		#region Naming

		[CommandLineArgument("name")] [ConVar("name", "Sets the name", true)]
		public static string StartPlayerName = "NotSet";

		[Command]
		private void CmdSetName(string newName)
		{
			username = newName;
		}

		#endregion
	}
}