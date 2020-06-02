using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTurretController : MonoBehaviour
{
    public float health;
    public float speed;
    public float damagePerSecond;
    public float range;
    public Transform pointShoot;
    public GameObject head;
    public SphereCollider impactZone;

    [HideInInspector] public bool placed;

    List<Collider> colliders = new List<Collider>();

    private bool hasTarget;
    private GameObject target;
    private Vector3 relativePosition;
    private float rotationTime;
    private Quaternion targetRotation;
    private bool rotating = false;

    // Start is called before the first frame update
    void Start()
    {
        impactZone.radius = range;
        GameManager.instance.AddActiveDefense(gameObject);
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
                        rotating = true;
                        rotationTime = 0;
                        return;
                    }
                }                
            }
            else
            {
                if (rotating)
                {
                    relativePosition = target.transform.position - head.transform.position;
                    targetRotation = Quaternion.LookRotation(relativePosition);
                    rotationTime += Time.deltaTime * speed;
                    head.transform.rotation = Quaternion.Lerp(head.transform.rotation, targetRotation, rotationTime);
                    if (head.transform.rotation == targetRotation) rotating = false;
                }
                else
                {
                    head.transform.LookAt(target.transform, Vector3.up);
                    RaycastHit hit;
                    if (Physics.Raycast(pointShoot.position, (target.transform.position - pointShoot.position).normalized, out hit, range, GameManager.instance.player.shootLayerMask))
                    {
                        GroundEnemy gEnemy = hit.collider.GetComponentInParent<GroundEnemy>();
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
                            TankEnemy tEnemy = hit.collider.GetComponentInParent<TankEnemy>();
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
                }                 
                if (Vector3.Distance(pointShoot.position, target.transform.position) > range) hasTarget = false;
            }            
        }        
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) DestroyDefenses();
    }

    public void DestroyDefenses()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponentInParent<GroundEnemy>() != null && collider.tag != "CriticalBox" && collider.gameObject.layer != LayerMask.NameToLayer("EnemyAttack")) colliders.Add(collider);
        if (collider.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
        {
            if (collider.gameObject.GetComponentInParent<GroundEnemy>() != null) TakeDamage(collider.gameObject.GetComponentInParent<GroundEnemy>().damage);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.GetComponentInParent<GroundEnemy>() != null && collider.tag != "CriticalBox" && collider.gameObject.layer != LayerMask.NameToLayer("EnemyAttack")) colliders.Remove(collider);
    }
}
