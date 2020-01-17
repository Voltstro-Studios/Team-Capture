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
		[SerializeField] private Behaviour[] disableBehaviourOnDeath;

		[SerializeField] private GameObject[] disableGameObjectsOnDeath;

		[SyncVar(hook = nameof(UpdateHealthUi))] private int health;

		[SyncVar] private int kills;
		[SyncVar] private int deaths;

		[SerializeField] private int maxHealth = 100;
		[SyncVar] public string username = "Not Set";

		public bool IsDead { get; protected set; }

		public int GetHealth => health;
		public int GetKills => kills;
		public int GetDeaths => deaths;

		[HideInInspector] public ClientUI clientUi;

		[SerializeField] private float latencyUpdateTime = 2.0f;

		/// <summary>
		/// Updated every X amount of seconds and haves this player's latency
		/// </summary>
		[SyncVar] public double latency;

		private bool isConnected;

		private void Start()
		{
			health = maxHealth;
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

		#region Death, Respawn, Damage

		[Server]
		public void TakeDamage(int damageAmount, string sourcePlayerId)
		{
			if (string.IsNullOrWhiteSpace(sourcePlayerId))
			{
				Logger.Log("The sourcePlayerId cannot be empty or null!", LogVerbosity.Error);
				return;
			}

			health -= damageAmount;

			if(health > 0) return;

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

			RpcClientPlayerDie();

			//Update the stats, for both players
			deaths++;
			GameManager.GetPlayer(sourcePlayerId).kills++;

			StartCoroutine(ServerPlayerRespawn());
		}

		private IEnumerator ServerPlayerRespawn()
		{
			yield return new WaitForSeconds(GameManager.Instance.scene.respawnTime);

			health = maxHealth;

			Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
			RpcClientRespawn(spawnPoint.position, spawnPoint.rotation);

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
				GetComponent<WeaponManager>().CmdRemoveAllWeapons();
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

				//TODO: Get stock weapons from server
				WeaponManager weaponManager = GetComponent<WeaponManager>();
				foreach (TCWeapon stockWeapon in GameManager.Instance.scene.stockWeapons)
					weaponManager.AddWeapon(stockWeapon.weapon);

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
			if(isLocalPlayer && clientUi != null)
				clientUi.hud.UpdateHealthUi();
		}

		#endregion

		private IEnumerator LatencyUpdateLoop()
		{
			while (isConnected)
			{
				latency = NetworkTime.rtt;
				yield return new WaitForSeconds(latencyUpdateTime);
			}
		}
	}
}