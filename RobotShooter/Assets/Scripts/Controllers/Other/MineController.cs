using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    public float damage;
    public float damageToPlayer;
    public float explosionRadius;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

            foreach (Collider nearbyObject in colliders)
            {
                /*Rigidbody rgb = nearbyObject.GetComponent<Rigidbody>();
                if (rgb != null)
                {
                    rgb.AddExplosionForce(force, transform.position, explosionRadius);
                }*/

                GroundEnemy gEnemy = nearbyObject.GetComponent<GroundEnemy>();
                if (gEnemy != null) gEnemy.TakeDamage(damage);
                else
                {
                    FlyingEnemy fEnemy = nearbyObject.GetComponent<FlyingEnemy>();
                    if (fEnemy != null) fEnemy.TakeDamage(damage);
                    else
                    {
                        PlayerController player = nearbyObject.GetComponent<PlayerController>();
                        if (player != null) player.TakeDamage(damageToPlayer, 0);
                    }
                }
                //Destroy(gameObject);
            }
            Destroy(gameObject);
        }
    }
}
