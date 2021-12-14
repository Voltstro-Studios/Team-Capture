using System;
using Mirror;
using Team_Capture.Player;
using Team_Capture.Player.Movement;
using UnityEngine;

namespace Team_Capture.Weapons.Projectiles
{
    public class ProjectileRocket : NetworkBehaviour
    {
        /// <summary>
        ///     How much force is applied to the rocket for it to go forward
        /// </summary>
        [SerializeField] private float appliedForce = 1200;

        /// <summary>
        ///     Explosion damage
        /// </summary>
        [SerializeField] private int explosionDamage = 70;

        /// <summary>
        ///     The size of the explosion
        /// </summary>
        [SerializeField] private float explosionSize = 6f;

        [SerializeField] private float explosionForce = 100f;

        [SerializeField] private float percentageRemoveOfOwner = 0.60f;
        
        /// <summary>
        ///     <see cref="LayerMask"/> of the explosion raycast
        /// </summary>
        [SerializeField] private LayerMask layerMask;
        
        /// <summary>
        ///     The explosion <see cref="GameObject"/> that is spawned when the rocket hits
        /// </summary>
        [SerializeField] private GameObject explosionPrefab;

        [SerializeField] private int colliderHitsBufferSize = 4;
        
        private Collider[] rayCastHits;
        private PlayerManager rocketOwner;

        public void Setup(PlayerManager owner)
        {
            rocketOwner = owner;
        }

        private void Start()
        {
            Vector3 force = appliedForce * transform.forward;
            
            //TODO: We should do the force stuff on the client as well, and not sync the pos, instead only sending a message on where it hit
            if(isServer)
            {
                GetComponent<Rigidbody>().AddForce(force);

                rayCastHits = new Collider[colliderHitsBufferSize];
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //Spawn explosion particle
            Instantiate(explosionPrefab, transform.position, transform.rotation);
            
            int size = Physics.OverlapSphereNonAlloc(transform.position, explosionSize, rayCastHits, layerMask);
            for (int i = 0; i < size; i++)
            {
                PlayerManager player = rayCastHits[i].GetComponent<PlayerManager>();
                if (player != null)
                {
                    player.GetComponent<PlayerMovementManager>().KnockBack(player.transform.position - transform.position, explosionForce);
                        
                    if(isServer)
                    {
                        int damage = explosionDamage;
                        if (player == rocketOwner)
                        {
                            float reducedDamage = damage * percentageRemoveOfOwner;
                            damage = Mathf.RoundToInt(reducedDamage);
                        }
                        player.TakeDamage(damage, rocketOwner.transform.name);
                    }
                }
            }

            if (isServer)
                NetworkServer.Destroy(gameObject);
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position, explosionSize);
        }

#endif
    }
}
