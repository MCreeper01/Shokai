using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [HideInInspector] public float damage;
    public float speed;
    public float timeToDestroy;
    float t;

    // Start is called before the first frame update
    void Start()
    {
        //Destroy(gameObject, timeToDestroy);
    }

    private void OnEnable()
    {
        t = timeToDestroy;
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
            PlayerController player = collider.GetComponent<PlayerController>();
            AudioManager.instance.PlayOneShotSound("ReceiveDamage", player.transform.position);
            player.TakeDamage(damage, 0);
            gameObject.SetActive(false);
        }
        if (collider.tag == "AirTurret")
        {
            collider.GetComponentInParent<AirTurretController>().TakeDamage(damage);
            gameObject.SetActive(false);
        }
        if (collider.tag == "GroundTurret")
        {
            collider.GetComponentInParent<TerrainTurretController>().TakeDamage(damage);
            gameObject.SetActive(false);
        }
        if (collider.gameObject.layer == LayerMask.NameToLayer("Geometry")) gameObject.SetActive(false);
    }
}
