using System;
using Team_Capture.Core;
using Team_Capture.Misc;
using Team_Capture.Player;
using UnityEngine;
using UnityEngine.VFX;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Weapons.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileRocket : ProjectileBase
    {
        /// <summary>
        ///     The explosion <see cref="GameObject"/> that is spawned when the rocket hits
        /// </summary>
        [SerializeField] private GameObject explosionPrefab;
        
        /// <summary>
        ///     How much force is applied to the rocket for it to go forward
        /// </summary>
        [Header("Physics")]
        [SerializeField]
        private float appliedForce = 1200;
        
        /// <summary>
        ///     The size of the explosion
        /// </summary>
        [SerializeField] private float explosionSize = 6f;

        /// <summary>
        ///     How much force to give players on explode
        /// </summary>
        [SerializeField] private float explosionForce = 100f;

        /// <summary>
        ///     Explosion damage
        /// </summary>
        [Header("Damage")]
        [SerializeField]
        private int explosionDamage = 70;
        
        /// <summary>
        ///     Whats the percentage of damage we will do to the owner of the rocket
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        private float percentageRemoveOfOwner = 0.20f;

        /// <summary>
        ///     <see cref="LayerMask"/> of the explosion raycast
        /// </summary>
        [Header("Explosion Raycast")]
        [SerializeField] private LayerMask layerMask;
        
        /// <summary>
        ///     Max hits we can do in a raycast
        /// </summary>
        [SerializeField] private int colliderHitsBufferSize = 4;

        /// <summary>
        ///     The VFX effect that will play for the trail
        /// </summary>
        [Header("Rocket Trail")]
        [SerializeField] private GameObject rocketTrailVfxPrefab;
        
        /// <summary>
        ///     The spawn point for where the VFX trail will spawn
        /// </summary>
        [SerializeField] private Transform rocketTrailSpawnPoint;

        /// <summary>
        ///     How long will the trail last for after it has done playing
        /// </summary>
        [SerializeField] private float rocketTrailDestroyTime = 6f;

        private Collider[] rayCastHits;
        private VisualEffect rocketTrail;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                Logger.Error("Rocket doesn't have a rigidbody attached to it!");
        }

        protected override void OnUserIdUpdate(string userId)
        {
            ProjectileOwner = GameManager.GetPlayer(userId);
        }

        protected override void Enable(Vector3 location, Vector3 rotation)
        {
            base.Enable(location, rotation);

            Vector3 force = appliedForce * transform.forward;
            rb.AddForce(force);
            
            rayCastHits = new Collider[colliderHitsBufferSize];

            rocketTrail = Instantiate(rocketTrailVfxPrefab, rocketTrailSpawnPoint).GetComponent<VisualEffect>();
        }

        protected override void Disable()
        {
            base.Disable();
            
            rb.velocity = Vector3.zero;
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

            //Return the object
            if (isServer)
            {
                ServerDisable();
                ServerReturnToPool();
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position, explosionSize);
        }

#endif
    }
}
