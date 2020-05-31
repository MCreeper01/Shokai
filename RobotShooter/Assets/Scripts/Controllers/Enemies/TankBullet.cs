using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBullet : MonoBehaviour
{
    [HideInInspector] public float damage;
    public float explosionRadius;
    public float timeToDestroyWithoutImpact;
    float t;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        //Destroy(gameObject, timeToDestroyWithoutImpact);
    }

    private void OnEnable()
    {
        t = timeToDestroyWithoutImpact;
    }

    // Update is called once per frame
    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0) gameObject.SetActive(false);

        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            collider.GetComponent<PlayerController>().TakeDamage(damage, 0);
        }
        if (collider.tag == "AirTurret")
        {
            collider.GetComponent<AirTurretController>().TakeDamage(damage);
        }
        if (collider.tag == "GroundTurret")
        {
            collider.GetComponent<TerrainTurretController>().TakeDamage(damage);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        Debug.Log("Yesss");
        for (int i = 0; i < colliders.Length; i++)
        {
            PlayerController pc = colliders[i].GetComponent<PlayerController>();

            if (pc != null) pc.TakeDamage(damage, 0);
            else
            {
                TerrainTurretController tTurret = colliders[i].GetComponent<TerrainTurretController>();
                if (tTurret != null) tTurret.TakeDamage(damage);
                else
                {
                    AirTurretController aTurret = colliders[i].GetComponent<AirTurretController>();
                    if (aTurret != null) aTurret.TakeDamage(damage);
                }
            }
        }
        gameObject.SetActive(false);
    }
}
