using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public class AirTurretController : MonoBehaviour
{
    public float health;
    public float speed;
    public float damagePerSecond;
    public float range;
    public Transform pointShoot;
    public GameObject head;
    public SphereCollider impactZone;
    public GameObject shootParticles;
    public GameObject explosionParticles;

    public bool placed;

    List<Collider> colliders = new List<Collider>();

    private bool hasTarget;
    private GameObject target;
    private Vector3 relativePosition;
    private float rotationTime;
    private Quaternion targetRotation;
    private bool rotating = false;
    private EventInstance shotSound;

    // Start is called before the first frame update
    void Start()
    {
        impactZone.radius = range;
        shootParticles.SetActive(false);
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
                shootParticles.SetActive(false);
                //AudioManager.instance.Stop(shotSound);
                foreach (Collider nearbyObject in colliders)
                {
                    target = nearbyObject.gameObject;
                    hasTarget = true;
                    rotating = true;
                    rotationTime = 0;
                    return;
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
                    shootParticles.SetActive(true);
                    head.transform.LookAt(target.transform, Vector3.up);
                    //shotSound = AudioManager.instance.PlayEvent("TurretShot", transform.position);

                    RaycastHit hit;
                    if (Physics.Raycast(pointShoot.position, (target.transform.position - pointShoot.position).normalized, out hit, range, GameManager.instance.player.shootLayerMask))
                    {
                        FlyingEnemy fEnemy = hit.collider.GetComponentInParent<FlyingEnemy>();
                        if (fEnemy != null)
                        {
                            fEnemy.TakeDamage(damagePerSecond * Time.deltaTime);
                            if (fEnemy.health <= 0)
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

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) DestroyDefenses();
    }

    public void DestroyDefenses()
    {
        Instantiate(explosionParticles, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collider)
    {

        if (collider.GetComponentInParent<FlyingEnemy>() != null && collider.tag != "CriticalBox") colliders.Add(collider); 
            if (collider.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
        {
            if (collider.gameObject.GetComponentInParent<FlyingEnemy>() != null) TakeDamage(collider.gameObject.GetComponentInParent<FlyingEnemy>().damage);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.GetComponentInParent<FlyingEnemy>() != null) colliders.Remove(collider);
    }
}
