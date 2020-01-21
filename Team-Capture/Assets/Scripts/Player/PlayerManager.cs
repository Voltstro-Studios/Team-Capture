using System.Collections;
using Global;
using Mirror;
using UI;
using UnityEngine;
using Weapons;
using Logger = Global.Logger;

namespace Player
{
	public class PlayerManager : NetworkBehaviour
	{
		[HideInInspector] public ClientUI clientUi;
		[SerializeField] private Behaviour[] disableBehaviourOnDeath;

		[SerializeField] private GameObject[] disableGameObjectsOnDeath;

		private bool isConnected;

		/// <summary>
		/// Updated every X amount of seconds and haves this player's latency
		/// </summary>
		[SyncVar] public double latency;

		[SerializeField] private float latencyUpdateTime = 2.0f;

		[SerializeField] private int maxHealth = 100;
		[SyncVar] public string username = "Not Set";

		public bool IsDead { get; protected set; }

		[field: SyncVar(hook = nameof(UpdateHealthUi))]
		public int GetHealth { get; private set; }

		[field: SyncVar] public int GetKills { get; private set; }

		[field: SyncVar] public int GetDeaths { get; private set; }

		private void Start()
		{
			GetHealth = maxHealth;
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
			TakeDamage(GetHealth, transform.name);
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

			GetHealth -= damageAmount;

			if (GetHealth > 0) return;

			//Player is dead
			ServerPlayerDie(sourcePlayerId);
		}

		/// <summary>
		/// The server side method that handles a player's death
		/// </summary>
		[Server]
		private void ServerPlayerDie(string sourcePlayerId)
		{
			IsDead = true;

			//Remove all the weapons on the player
			GetComponent<WeaponManager>().RemoveAllWeapons();

			RpcClientPlayerDie();

			//Update the stats, for both players
			GetDeaths++;
			if (sourcePlayerId != transform.name)
				GameManager.GetPlayer(sourcePlayerId).GetKills++;

			StartCoroutine(ServerPlayerRespawn());
		}

		[Server]
		private IEnumerator ServerPlayerRespawn()
		{
			yield return new WaitForSeconds(GameManager.Instance.scene.respawnTime);

			GetHealth = maxHealth;

			Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
			RpcClientRespawn(spawnPoint.position, spawnPoint.rotation);

			//Add scene stock weapons
			WeaponManager weaponManager = GetComponent<WeaponManager>();
			foreach (TCWeapon stockWeapon in GameManager.Instance.scene.stockWeapons)
				weaponManager.ServerAddWeapon(stockWeapon.weapon);

			IsDead = false;
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
				GameManager.Instance.sceneCamera.SetActive(true);

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
				GameManager.Instance.sceneCamera.SetActive(false);

				clientUi.hud.gameObject.SetActive(true);
				clientUi.hud.UpdateHealthUi();
			}
			else
			{
				GetComponent<CapsuleCollider>().enabled = true;
			}
		}

		private void UpdateHealthUi(int oldHealth, int newHealth)
		{
			if (isLocalPlayer && clientUi != null)
				clientUi.hud.UpdateHealthUi();
		}

		#endregion
	}
}