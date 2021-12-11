using System;
using Mirror;
using UnityEngine;

namespace Team_Capture.Weapons.Projectiles
{
    public class ProjectileRocket : NetworkBehaviour
    {
        public float appliedForce = 100;
        
        private void Start()
        {
            Vector3 force = appliedForce * transform.forward;
            
            if(isServer)
                GetComponent<Rigidbody>().AddForce(force);
        }

        private void OnTriggerEnter(Collider other)
        {
            //Spawn explosion particle
            
            //TODO: We should use a game object pool
            Destroy(gameObject);
        }
    }
}
