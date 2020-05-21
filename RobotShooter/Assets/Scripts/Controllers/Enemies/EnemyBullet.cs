using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [HideInInspector] public float damage;
    public float speed;
    public float timeToDestroy;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            player.TakeDamage(damage, 0);
            Destroy(gameObject);
        }
        if (collider.tag == "AirTurret")
        {
            collider.GetComponent<AirTurretController>().TakeDamage(damage);
            Destroy(gameObject);
        }
        if (collider.tag == "GroundTurret")
        {
            collider.GetComponent<TerrainTurretController>().TakeDamage(damage);
            Destroy(gameObject);
        }
        
    }
}
