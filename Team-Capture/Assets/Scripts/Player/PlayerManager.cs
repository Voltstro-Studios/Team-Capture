using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Weapons;

namespace Player
{
    public class PlayerManager : NetworkBehaviour
    {
        [SyncVar] public string username = "Not Set";

		[SerializeField] private int maxHealth = 100;
		[SyncVar] private int health;

		[SerializeField] private GameObject[] disableGameObjectsOnDeath;

		public bool IsDead { get; protected set; }

		private WeaponManager localWeaponManager;
		private PlayerWeaponShoot localPlayerWeaponShoot;
		private PlayerMovement localPlayerMovement;

		private void Start()
		{
			health = maxHealth;

			localWeaponManager = GetComponent<WeaponManager>();
			localPlayerWeaponShoot = GetComponent<PlayerWeaponShoot>();
			localPlayerMovement = GetComponent<PlayerMovement>();
		}

		private void Update()
		{
			//This is temp
			if (Input.GetKeyDown(KeyCode.G))
			{
				CmdTakeDamage(transform.name, 100000);
			}
		}

		[Command]
		public void CmdTakeDamage(string sourceId, int damage)
		{
			PlayerManager player = GameManager.GetPlayer(sourceId);
			if(player == null)
				return;

			player.health -= damage;

			if (player.health <= 0)
			{
				//Dead
				Debug.Log("Dead");

				RpcDie();
			}
		}

		[ClientRpc]
		private void RpcDie()
		{
			IsDead = true;

			if(isLocalPlayer)
				localWeaponManager.CmdRemoveAllWeapons();

			localWeaponManager.enabled = false;
			localPlayerWeaponShoot.enabled = false;

			foreach (GameObject toDisable in disableGameObjectsOnDeath)
			{
				toDisable.SetActive(false);
			}

			//Disable the collider, or the Char controller
			if (isLocalPlayer)
			{
				localPlayerMovement.enabled = false;
				GetComponent<CharacterController>().enabled = false;

				//Switch cams
				GameManager.Instance.sceneCamera.SetActive(true);
			}
			else
			{
				GetComponent<CapsuleCollider>().enabled = false;
			}

			StartCoroutine(Respawn());
		}

		private IEnumerator Respawn()
		{
			yield return new WaitForSeconds(GameManager.Instance.scene.respawnTime);

			localWeaponManager.enabled = true;
			localPlayerWeaponShoot.enabled = true;

			//Enable game objects
			foreach (GameObject gameObjectToDisable in disableGameObjectsOnDeath)
			{
				gameObjectToDisable.SetActive(true);
			}

			//Enable the collider, or the Char controller
			if (isLocalPlayer)
			{
				localPlayerMovement.enabled = true;
				GetComponent<CharacterController>().enabled = true;

				//Switch cams
				GameManager.Instance.sceneCamera.SetActive(false);

				foreach (TCWeapon stockWeapon in GameManager.Instance.scene.stockWeapons)
				{
					localWeaponManager.AddWeapon(stockWeapon.weapon);
				}
			}
			else
			{
				GetComponent<CapsuleCollider>().enabled = true;
			}

			health = maxHealth;
			IsDead = false;
		}
    }
}
