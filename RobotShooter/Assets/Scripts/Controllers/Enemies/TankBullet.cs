using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBullet : MonoBehaviour
{
    public float explosionRadius;
    public float damage;
    public float timeToDestroyWithoutImpact;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timeToDestroyWithoutImpact);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {        
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        for (int i = 0; i < colliders.Length; i++)
        {
            PlayerController pc = colliders[i].GetComponent<PlayerController>();

            if (pc != null)
            {
                
                pc.TakeDamage(damage, 0);
            }
        }
        Destroy(gameObject);
    }
}
