using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeController : MonoBehaviour
{
    //public GameObject explosionEffect;

    public float delay;
    public float explosionRadius;
    public float force;
    public float damage;

    private float countdownDelay;
    private bool hasExploded = false;

    // Start is called before the first frame update
    void Start()
    {
        countdownDelay = delay;
    }

    // Update is called once per frame
    void Update()
    {
        countdownDelay -= Time.deltaTime;
        if (countdownDelay <= 0 && !hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;
        //Instantiate(explosionEffect, transform.position, transform.rotation);

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
                    Enemy3 tEnemy = nearbyObject.GetComponent<Enemy3>();
                    if (tEnemy != null) tEnemy.TakeDamage(damage);
                }
            }
            //Destroy(gameObject);
        }
        Destroy(gameObject);        
    }
}
