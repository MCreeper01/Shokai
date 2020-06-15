using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyGrenadeController : MonoBehaviour
{
    public float delay;
    public float explosionRadius;
    public float force;
    public float maxDamage;
    public float maxDamageToPlayer;
    public GameObject explosionParticles;
    public GameObject explosionDecal;
    public LayerMask groundLayer;

    private float countdownDelay;
    private bool canExplode = false;
    private bool hasExploded = false;
    private float beepTime;
    private float timesOfBeep;

    // Start is called before the first frame update
    void Start()
    {
        countdownDelay = delay;
        beepTime = 1;
        timesOfBeep = 1;
        StartCoroutine(TimeBeep());
    }

    // Update is called once per frame
    void Update()
    {
        if (canExplode) countdownDelay -= Time.deltaTime;
        if (countdownDelay <= 0 && !hasExploded)
        {
            StopCoroutine(TimeBeep());
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
            float dist = Vector3.Distance(transform.position, nearbyObject.transform.position);
            float ratio = Mathf.Clamp01(1 - dist / explosionRadius);

            GroundEnemy gEnemy = nearbyObject.GetComponentInParent<GroundEnemy>();
            if (gEnemy != null) gEnemy.TakeDamage(maxDamage * ratio);
            else
            {
                FlyingEnemy fEnemy = nearbyObject.GetComponentInParent<FlyingEnemy>();
                if (fEnemy != null) fEnemy.TakeDamage(maxDamage * ratio);
                else
                {
                    TankEnemy tEnemy = nearbyObject.GetComponentInParent<TankEnemy>();
                    if (tEnemy != null) tEnemy.TakeDamage(maxDamage * ratio);
                    else
                    {
                        PlayerController player = nearbyObject.GetComponent<PlayerController>();
                        if (player != null) player.TakeDamage(maxDamageToPlayer * ratio, 0);
                    }
                }
            }
        }
        AudioManager.instance.PlayOneShotSound("GranadeExplosion", transform.position);
        Instantiate(explosionParticles, transform.position, transform.rotation);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 0.5f, groundLayer))
        {
            GameObject explosionDecalGO = Instantiate(explosionDecal, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.LookRotation(hit.normal));

            explosionDecalGO.transform.parent = hit.transform;
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

    IEnumerator TimeBeep()
    {
        while (!hasExploded)
        {
            for (int i = 0; i < timesOfBeep; i++)
            {
                AudioManager.instance.PlayOneShotSound("CountdownGrenades", transform.position);
                yield return new WaitForSeconds(beepTime);                
            }
            beepTime /= 2;
        }
    }
}
