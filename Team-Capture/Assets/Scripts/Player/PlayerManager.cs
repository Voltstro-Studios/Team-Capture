using System.Collections;
using Core;
using Core.Logger;
using Core.Networking.Messages;
using Delegates;
using Mirror;
using UI;
using UnityEngine;
using Weapons;
using Logger = Core.Logger.Logger;

namespace Player
{
	public class PlayerManager : NetworkBehaviour
	{
		[HideInInspector] public ClientUI clientUi;
		[SerializeField] private Behaviour[] disableBehaviourOnDeath;

		[SerializeField] private GameObject[] disableGameObjectsOnDeath;

		[SerializeField] private float invincibilityLastTime = 3.0f;
		[SerializeField] private float latencyUpdateTime = 2.0f;

		private bool isConnected;

		public int MaxHealth { get; } = 100;

		#region Sync Vars

		[field: SyncVar] public bool IsDead { get; protected set; }

		[SyncVar] public string username = "Not Set";

		[field: SyncVar(hook = nameof(UpdateHealthUi))]
		public int Health { get; private set; }

		[field: SyncVar] public int Kills { get; private set; }

		[field: SyncVar] public int Deaths { get; private set; }

		/// <summary>
		/// Updated every X amount of seconds and haves this player's latency
		/// </summary>
		//TODO: We gotta find a better way of getting a client's latency
		[SyncVar] public double latency;

		[SyncEvent] public event PlayerKilledDelegate EventPlayerKilled;

		#endregion

		#region Server Variables

		/// <summary>
		/// Only set on the server!
		/// <para>The player's active <see cref="WeaponManager"/></para>
		/// </summary>
		private WeaponManager weaponManager;

		/// <summary>
		/// Only set on the server!
		/// <para>Is this player invincible?</para>
		/// </summary>
		public bool IsInvincible { get; private set; }
		
		#endregion

		public override void OnStartServer()
		{
			base.OnStartServer();

			Health = MaxHealth;
			weaponManager = GetComponent<WeaponManager>();
		}

		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();

			isConnected = true;

			StartCoroutine(LatencyUpdateLoop());
		}

		public override void OnStopAuthority()
		{
			base.OnStopAuthority();

			isConnected = false;
		}

		[Command]
		public void CmdSuicide()
		{
			TakeDamage(Health, transform.name);
		}

		private IEnumerator LatencyUpdateLoop()
		{
			while (isConnected)
			{
				latency = NetworkTime.rtt;
				yield return new WaitForSeconds(latencyUpdateTime);
			}
		}

		#region Death, Respawn, Damage

		[Server]
		public void TakeDamage(int damageAmount, string sourcePlayerId)
		{
			if (string.IsNullOrWhiteSpace(sourcePlayerId))
			{
				Logger.Log("The sourcePlayerId cannot be empty or null!", LogVerbosity.Error);
				return;
			}

			if(IsDead) return;

			if(IsInvincible) return;

			Health -= damageAmount;

			if (Health > 0) return;

			//Player is dead
			ServerPlayerDie(sourcePlayerId);
		}

		[Server]
		public void AddHealth(int amount)
		{
			if(Health == amount)
				return;

			if (Health + amount > MaxHealth)
			{
				Health = MaxHealth;
				return;
			}

			Health += amount;
		}

		/// <summary>
		/// The server side method that handles a player's death
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
				WeaponName = weaponManager.GetActiveWeapon().weapon
			});

			//Remove all the weapons on the player
			weaponManager.RemoveAllWeapons();

			RpcClientPlayerDie();

			//Update the stats, for both players
			PlayerManager killer = GameManager.GetPlayer(sourcePlayerId);
			Deaths++;
			if (sourcePlayerId != transform.name)
				killer.Kills++;

			EventPlayerKilled?.Invoke(transform.name, sourcePlayerId);

			StartCoroutine(ServerPlayerRespawn());
		}

		[Server]
		private IEnumerator ServerPlayerRespawn()
		{
			yield return new WaitForSeconds(GameManager.GetActiveScene().respawnTime);

			Health = MaxHealth;

			Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
			RpcClientRespawn(spawnPoint.position, spawnPoint.rotation);

			//Add stock weapons
			GetComponent<WeaponManager>().AddStockWeapons();

			IsDead = false;

			IsInvincible = true;
			yield return new WaitForSeconds(invincibilityLastTime);

			IsInvincible = false;
		}

		/// <summary>
		/// Disables components on each of the clients
		/// </summary>
		[ClientRpc]
		private void RpcClientPlayerDie()
		{
			//Disable the collider, or the Char controller
			if (isLocalPlayer)
			{
				GetComponent<PlayerMovement>().enabled = false;
				GetComponent<CharacterController>().enabled = false;

				//Switch cams
				GameManager.GetActiveSceneCamera().SetActive(true);

				//Disable the UI
				clientUi.hud.gameObject.SetActive(false);
			}
			else
			{
				GetComponent<CapsuleCollider>().enabled = false;
			}

			foreach (GameObject toDisable in disableGameObjectsOnDeath) toDisable.SetActive(false);

			foreach (Behaviour toDisable in disableBehaviourOnDeath) toDisable.enabled = false;
		}

		[ClientRpc]
		private void RpcClientRespawn(Vector3 spawnPos, Quaternion spawnRot)
		{
			transform.position = spawnPos;
			// ReSharper disable once Unity.InefficientPropertyAccess
			transform.rotation = spawnRot;

			//Enable game objects
			foreach (GameObject toEnable in disableGameObjectsOnDeath) toEnable.SetActive(true);

			foreach (Behaviour toEnable in disableBehaviourOnDeath) toEnable.enabled = true;

			//Enable the collider, or the Char controller
			if (isLocalPlayer)
			{
				GetComponent<CharacterController>().enabled = true;
				GetComponent<PlayerMovement>().enabled = true;

				//Switch cams
				GameManager.GetActiveSceneCamera().SetActive(false);

				//Enable our UI
				clientUi.hud.gameObject.SetActive(true);
			}
			else
			{
				GetComponent<CapsuleCollider>().enabled = true;
			}
		}

#pragma warning disable IDE0060 // Remove unused parameter, yes these paramaters HAVE to be here for the hook! And yea we gotta do it for both ReSharper and VS
		// ReSharper disable UnusedParameter.Local
		private void UpdateHealthUi(int oldHealth, int newHealth)
			// ReSharper restore UnusedParameter.Local
		{
			if (isLocalPlayer && clientUi != null)
				clientUi.hud.UpdateHealthUi();
		}
#pragma warning restore IDE0060 // Remove unused parameter

		#endregion
	}
}