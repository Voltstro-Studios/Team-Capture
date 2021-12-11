using Mirror;
using UnityEngine;

namespace Team_Capture.Weapons.Projectiles
{
    public class ProjectileRocket : NetworkBehaviour
    {
        public float appliedForce = 100;

        public void AddForce(Vector3 force)
        {
            
        }
        
        private void Start()
        {
            Vector3 force = appliedForce * transform.forward;
            
            if(isServer)
                GetComponent<Rigidbody>().AddForce(force);
        }
    }
}
