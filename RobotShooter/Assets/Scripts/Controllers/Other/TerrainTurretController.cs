using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTurretController : MonoBehaviour
{
    public float damagePerSecond;
    public float range;
    public Transform pointShoot;
    public GameObject head;
    public SphereCollider impactZone;

    [HideInInspector] public bool placed;

    List<Collider> colliders = new List<Collider>();

    private bool hasTarget;
    private GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        impactZone.radius = range;
    }

    // Update is called once per frame
    void Update()
    {
        if (placed)
        {
            colliders.RemoveAll(col => col == null);
            if (target == null) hasTarget = false;
            if (!hasTarget)
            {
                if (colliders.Count > 0)
                {
                    foreach (Collider nearbyObject in colliders)
                    {
                        target = nearbyObject.gameObject;
                        hasTarget = true;
                        return;
                    }
                }                
            }
            else
            {
                head.transform.LookAt(target.transform, Vector3.up);

                RaycastHit hit;
                if (Physics.Raycast(pointShoot.position, (target.transform.position - pointShoot.position).normalized, out hit, range, GameManager.instance.player.shootLayerMask))
                {
                    GroundEnemy gEnemy = hit.collider.GetComponent<GroundEnemy>();
                    if (gEnemy != null)
                    {
                        gEnemy.TakeDamage(damagePerSecond * Time.deltaTime);
                        if (gEnemy.health <= 0)
                        {
                            hasTarget = false;
                            colliders.Remove(target.GetComponent<Collider>());
                        }
                    }
                    else
                    {
                        Enemy3 tEnemy = hit.collider.GetComponent<Enemy3>();
                        if (tEnemy != null)
                        {
                            tEnemy.TakeDamage(damagePerSecond * Time.deltaTime);
                            if (tEnemy.health <= 0)
                            {
                                hasTarget = false;
                                colliders.Remove(target.GetComponent<Collider>());
                            }
                        }
                        else hasTarget = false;
                    }
                }
                if (Vector3.Distance(pointShoot.position, target.transform.position) > range) hasTarget = false;
            }            
        }        
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponent<GroundEnemy>() != null) colliders.Add(collider);        
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.GetComponent<GroundEnemy>() != null) colliders.Remove(collider);
    }
}
