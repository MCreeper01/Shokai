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
        if (t <= 0)
        {
            gameObject.SetActive(false);
        } 

        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider collider)
    {
        AudioManager.instance.PlayOneShotSound("PlasmaBlast", transform.position);
        if (collider.tag == "Player")
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            player.TakeDamage(damage, 0);
            AudioManager.instance.PlayOneShotSound("ReceiveDamage", player.transform.position);
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            for (int i = 0; i < colliders.Length; i++)
            {
                PlayerController pc = colliders[i].GetComponent<PlayerController>();

                if (pc != null) pc.TakeDamage(damage, 0);
                else
                {
                    TerrainTurretController tTurret = colliders[i].GetComponentInParent<TerrainTurretController>();
                    if (tTurret != null) tTurret.TakeDamage(damage);
                    else
                    {
                        AirTurretController aTurret = colliders[i].GetComponentInParent<AirTurretController>();
                        if (aTurret != null) aTurret.TakeDamage(damage);
                    }
                }
            }
            gameObject.SetActive(false);
        }
        if (collider.tag == "AirTurret")
        {
            collider.GetComponentInParent<AirTurretController>().TakeDamage(damage);
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            for (int i = 0; i < colliders.Length; i++)
            {
                PlayerController pc = colliders[i].GetComponent<PlayerController>();

                if (pc != null) pc.TakeDamage(damage, 0);
                else
                {
                    TerrainTurretController tTurret = colliders[i].GetComponentInParent<TerrainTurretController>();
                    if (tTurret != null) tTurret.TakeDamage(damage);
                    else
                    {
                        AirTurretController aTurret = colliders[i].GetComponentInParent<AirTurretController>();
                        if (aTurret != null) aTurret.TakeDamage(damage);
                    }
                }
            }
            gameObject.SetActive(false);
        }
        if (collider.tag == "GroundTurret")
        {
            collider.GetComponentInParent<TerrainTurretController>().TakeDamage(damage);
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            for (int i = 0; i < colliders.Length; i++)
            {
                PlayerController pc = colliders[i].GetComponent<PlayerController>();

                if (pc != null) pc.TakeDamage(damage, 0);
                else
                {
                    TerrainTurretController tTurret = colliders[i].GetComponentInParent<TerrainTurretController>();
                    if (tTurret != null) tTurret.TakeDamage(damage);
                    else
                    {
                        AirTurretController aTurret = colliders[i].GetComponentInParent<AirTurretController>();
                        if (aTurret != null) aTurret.TakeDamage(damage);
                    }
                }
            }
            gameObject.SetActive(false);
        }
        if (collider.gameObject.layer == LayerMask.NameToLayer("Geometry"))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            for (int i = 0; i < colliders.Length; i++)
            {
                PlayerController pc = colliders[i].GetComponent<PlayerController>();

                if (pc != null) pc.TakeDamage(damage, 0);
                else
                {
                    TerrainTurretController tTurret = colliders[i].GetComponentInParent<TerrainTurretController>();
                    if (tTurret != null) tTurret.TakeDamage(damage);
                    else
                    {
                        AirTurretController aTurret = colliders[i].GetComponentInParent<AirTurretController>();
                        if (aTurret != null) aTurret.TakeDamage(damage);
                    }
                }
            }
            gameObject.SetActive(false);
        } 

               
    }
}
