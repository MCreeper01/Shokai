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
        GameManager.instance.AddActiveDefense(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyDefenses()
    {
        Destroy(gameObject);
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

                GroundEnemy gEnemy = nearbyObject.GetComponentInParent<GroundEnemy>();
                if (gEnemy != null) gEnemy.TakeDamage(damage);
                else
                {
                    FlyingEnemy fEnemy = nearbyObject.GetComponentInParent<FlyingEnemy>();
                    if (fEnemy != null) fEnemy.TakeDamage(damage);
                    else
                    {
                        PlayerController player = nearbyObject.GetComponent<PlayerController>();
                        if (player != null) player.TakeDamage(damageToPlayer, 0);
                        else
                        {
                            Enemy3 tEnemy = nearbyObject.GetComponent<Enemy3>();
                            if (tEnemy != null) tEnemy.TakeDamage(damage);
                        }
                    }
                }
                //Destroy(gameObject);
            }
            Destroy(gameObject);
        }
    }
}
