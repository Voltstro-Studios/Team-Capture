using System.Collections;
using Global;
using UnityEngine;
using Mirror;
using Weapons;
using Logger = Global.Logger;

namespace Player
{
    public class PlayerManager : NetworkBehaviour
    {
        [SyncVar] public string username = "Not Set";

		[SerializeField] private int maxHealth = 100;
		[SyncVar] private int health;

		[SerializeField] private GameObject[] disableGameObjectsOnDeath;
		[SerializeField] private Behaviour[] disableBehaviourOnDeath;

		public bool IsDead { get; protected set; }

		private void Start()
		{
			health = maxHealth;
		}

		[ClientRpc]
		public void RpcTakeDamage(string sourceId, int damage)
		{
			health -= damage;

			if(health <= 0)
				Die();
		}

		private void Die()
		{
			Logger.Log($"{transform.name} died.");

			IsDead = true;

			//Disable the collider, or the Char controller
			if (isLocalPlayer)
			{
				GetComponent<WeaponManager>().CmdRemoveAllWeapons();
				GetComponent<PlayerMovement>().enabled = false;
				GetComponent<CharacterController>().enabled = false;

				//Switch cams
				GameManager.Instance.sceneCamera.SetActive(true);
			}
			else
			{
				GetComponent<CapsuleCollider>().enabled = false;
			}

			foreach (GameObject toDisable in disableGameObjectsOnDeath)
			{
				toDisable.SetActive(false);
			}

			foreach (Behaviour toDisable in disableBehaviourOnDeath)
			{
				toDisable.enabled = false;
			}

			StartCoroutine(Respawn());
		}

		private IEnumerator Respawn()
		{
			yield return new WaitForSeconds(GameManager.Instance.scene.respawnTime);

			//Enable game objects
			foreach (GameObject toEnable in disableGameObjectsOnDeath)
			{
				toEnable.SetActive(true);
			}

			foreach (Behaviour toEnable in disableBehaviourOnDeath)
			{
				toEnable.enabled = true;
			}

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
				{
					weaponManager.AddWeapon(stockWeapon.weapon);
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
