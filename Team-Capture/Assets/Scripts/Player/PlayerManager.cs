using System.Collections;
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
		[SyncVar] private int health;

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
			RpcTakeDamage(transform.name, health);
		}

		#region Death, Respawn, Damage

		[ClientRpc]
		public void RpcTakeDamage(string sourceId, int damage)
		{
			health -= damage;

			if(isLocalPlayer)
				clientUi.hud.UpdateHealthUi();

			if (health > 0) return;

			Die();

			if (sourceId != transform.name)
				GameManager.GetPlayer(sourceId).kills++;

		}

		private void Die()
		{
			Logger.Log($"{transform.name} died.");

			IsDead = true;

			//Disable the collider, or the Char controller
			if (isLocalPlayer)
			{
				deaths++;

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

			StartCoroutine(Respawn());
		}

		private IEnumerator Respawn()
		{
			yield return new WaitForSeconds(GameManager.Instance.scene.respawnTime);

			Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
			transform.position = spawnPoint.position;
			transform.rotation = spawnPoint.rotation;

			yield return new WaitForSeconds(0.1f);

			health = maxHealth;

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

			IsDead = false;
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