using Mirror;
using Team_Capture.Core;
using Team_Capture.Misc;
using Team_Capture.Player;
using UnityEngine;
using UnityEngine.VFX;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Weapons.Projectiles
{
    public class ProjectileRocket : ProjectileBase
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

        /// <summary>
        ///     How much force to give players on explode
        /// </summary>
        [SerializeField] private float explosionForce = 100f;

        /// <summary>
        ///     Whats the percentage of damage we will do to the owner of the rocket
        /// </summary>
        [SerializeField] private float percentageRemoveOfOwner = 0.60f;
        
        /// <summary>
        ///     <see cref="LayerMask"/> of the explosion raycast
        /// </summary>
        [SerializeField] private LayerMask layerMask;
        
        /// <summary>
        ///     The explosion <see cref="GameObject"/> that is spawned when the rocket hits
        /// </summary>
        [SerializeField] private GameObject explosionPrefab;

        /// <summary>
        ///     Max hits we can do in a raycast
        /// </summary>
        [SerializeField] private int colliderHitsBufferSize = 4;

        [SerializeField] private VisualEffect rocketTrail;

        [SerializeField] private float rocketTrailDestroyTime = 6f;

        private Collider[] rayCastHits;
        
        protected override void Setup()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Logger.Error("Rocket doesn't have a rigidbody attached to it!");
                return;
            }

            Vector3 force = appliedForce * transform.forward;
            rb.AddForce(force);
            
            rayCastHits = new Collider[colliderHitsBufferSize];
        }

        private void OnTriggerEnter(Collider other)
        {
            //Don't explode on the player who owns this rocket
            if(other == ProjectileOwner.playerMovementManager.CharacterController)
                return;
            
            Transform rocketTransform = transform;
            Vector3 rocketPosition = rocketTransform.position;
            
            //Spawn explosion particle
            Instantiate(explosionPrefab, rocketPosition, rocketTransform.rotation);
            
            int size = Physics.OverlapSphereNonAlloc(rocketPosition, explosionSize, rayCastHits, layerMask);
            for (int i = 0; i < size; i++)
            {
                PlayerManager player = rayCastHits[i].GetComponent<PlayerManager>();
                if (player == null) 
                    continue;
                
                //We do knock-back, do it both on the local client and server
                player.playerMovementManager.KnockBack(player.transform.position - rocketPosition, explosionForce);

                if (!isServer) 
                    continue;
                
                //Whoever is the owner of this rocket will have reduced damage done to them
                int damage = explosionDamage;
                if (player == ProjectileOwner)
                {
                    float reducedDamage = damage * percentageRemoveOfOwner;
                    damage = Mathf.RoundToInt(reducedDamage);
                }
                player.TakeDamage(damage, ProjectileOwner.transform.name);
            }
            
            //Change the rocket trail parent
            if (!Game.IsHeadless)
            {
                rocketTrail.Stop();
                rocketTrail.transform.SetParent(null);
                rocketTrail.gameObject.AddComponent<TimedDestroyer>().destroyDelayTime = rocketTrailDestroyTime;
            }

            //Destroy this object
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
