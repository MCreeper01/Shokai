using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyGrenadeController : MonoBehaviour
{
    public float delay;
    public float explosionRadius;
    public float force;
    public float damage;

    private float countdownDelay;
    private bool canExplode = false;
    private bool hasExploded = false;

    // Start is called before the first frame update
    void Start()
    {
        countdownDelay = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (canExplode) countdownDelay -= Time.deltaTime;
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

            GroundEnemy gEnemy = nearbyObject.GetComponent<GroundEnemy>();
            if (gEnemy != null) gEnemy.TakeDamage(damage);
            else
            {
                FlyingEnemy fEnemy = nearbyObject.GetComponent<FlyingEnemy>();
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

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            transform.parent = col.gameObject.transform;
            canExplode = true;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        }        
    }
}
