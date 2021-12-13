using System;
using Mirror;
using UnityEngine;

namespace Team_Capture.Weapons.Projectiles
{
    public class ProjectileRocket : NetworkBehaviour
    {
        public float appliedForce = 100;

        public GameObject explosionPrefab;
        
        private void Start()
        {
            Vector3 force = appliedForce * transform.forward;
            
            if(isServer)
                GetComponent<Rigidbody>().AddForce(force);
        }

        private void OnTriggerEnter(Collider other)
        {
            //Spawn explosion particle
            Instantiate(explosionPrefab, transform.position, transform.rotation);
            
            if(isServer)
                NetworkServer.Destroy(gameObject);
        }
    }
}
