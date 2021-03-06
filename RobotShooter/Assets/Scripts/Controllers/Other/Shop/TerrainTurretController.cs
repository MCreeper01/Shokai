﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public class TerrainTurretController : MonoBehaviour
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
    public float angleThreshold;
    public float smokeHealthPercentage;

    public ParticleSystem smoke;

    [HideInInspector] public bool placed;

    List<Collider> colliders = new List<Collider>();

    private float maxHealth;
    private int colliderTarget;
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
        maxHealth = health;
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
                shotSound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                if (colliders.Count > 0)
                {
                    int num = 0;
                    float distance = Vector3.Distance(gameObject.transform.position, colliders[num].transform.position);
                    for (int i = 1; i < colliders.Count; i++)
                    {
                        if (Vector3.Distance(gameObject.transform.position, colliders[0].transform.position) < distance)
                        {
                            num = i;
                            distance = Vector3.Distance(gameObject.transform.position, colliders[num].transform.position);
                        }                    
                    }
                    colliderTarget = num;
                    target = colliders[num].gameObject;
                    hasTarget = true;
                    rotating = true;
                    rotationTime = 0;
                }
            }
            else
            {
                if (rotating)
                {
                    shotSound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    relativePosition = target.transform.position - head.transform.position;
                    targetRotation = Quaternion.LookRotation(relativePosition);
                    head.transform.rotation = Quaternion.Lerp(head.transform.rotation, targetRotation, speed * Time.deltaTime);
                    if (Quaternion.Angle(head.transform.rotation, targetRotation) < angleThreshold &&
                        Quaternion.Angle(head.transform.rotation, targetRotation) > -angleThreshold) rotating = false;
                }
                else
                {
                    shootParticles.SetActive(true);
                    head.transform.LookAt(target.transform, Vector3.up);
                    if (!AudioManager.instance.isPlaying(shotSound))
                    {
                        shotSound = AudioManager.instance.PlayOneShotSound("TurretShot", transform.position);
                    }

                    RaycastHit hit;
                    if (Physics.Raycast(pointShoot.position, (target.transform.position - pointShoot.position).normalized, out hit, range, GameManager.instance.player.shootLayerMask))
                    {
                        GroundEnemy gEnemy = hit.collider.GetComponentInParent<GroundEnemy>();
                        if (gEnemy != null)
                        {
                            gEnemy.TakeDamage(damagePerSecond * Time.deltaTime, 1);
                            if (gEnemy.health <= 0)
                            {
                                hasTarget = false;
                                colliders.RemoveAt(colliderTarget);
                            }
                        }
                        else
                        {
                            TankEnemy tEnemy = hit.collider.GetComponentInParent<TankEnemy>();
                            if (tEnemy != null)
                            {
                                tEnemy.TakeDamage(damagePerSecond * Time.deltaTime, 1);
                                if (tEnemy.health <= 0)
                                {
                                    hasTarget = false;
                                    colliders.RemoveAt(colliderTarget);
                                }
                            }
                            else
                            {
                                colliders.RemoveAt(colliderTarget);
                                hasTarget = false;
                            } 
                        }
                    }
                    else
                    {
                        colliders.RemoveAt(colliderTarget);
                        shotSound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                        colliders.Remove(target.GetComponent<Collider>());
                        hasTarget = false;                        
                    } 
                }                 
                if (Vector3.Distance(pointShoot.position, target.transform.position) > range) hasTarget = false;
            }            
        }        
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if ((health <= maxHealth * smokeHealthPercentage / 100) && !smoke.isPlaying) smoke.Play();
        if (health <= 0) DestroyDefenses();
    }

    public void DestroyDefenses()
    {
        shotSound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        AudioManager.instance.PlayOneShotSound("TurretExplosion", transform.position);
        Instantiate(explosionParticles, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponentInParent<GroundEnemy>() != null && collider.tag != "CriticalBox" && collider.gameObject.layer != LayerMask.NameToLayer("EnemyAttack")) colliders.Add(collider);
        if (collider.GetComponentInParent<TankEnemy>() != null && collider.tag != "CriticalBox" && collider.gameObject.layer != LayerMask.NameToLayer("EnemyAttack")) colliders.Add(collider);
        if (collider.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
        {
            if (collider.gameObject.GetComponentInParent<GroundEnemy>() != null) TakeDamage(collider.gameObject.GetComponentInParent<GroundEnemy>().damage);
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.GetComponentInParent<GroundEnemy>() != null && collider.tag != "CriticalBox" && collider.gameObject.layer != LayerMask.NameToLayer("EnemyAttack"))
        {
            foreach (Collider col in colliders) if (collider == col) return;
            colliders.Add(collider);
        }
        if (collider.GetComponentInParent<TankEnemy>() != null && collider.tag != "CriticalBox" && collider.gameObject.layer != LayerMask.NameToLayer("EnemyAttack"))
        {
            foreach (Collider col in colliders) if (collider == col) return;
            colliders.Add(collider);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.GetComponentInParent<GroundEnemy>() != null && collider.tag != "CriticalBox" && collider.gameObject.layer != LayerMask.NameToLayer("EnemyAttack")) colliders.Remove(collider);
        if (collider.GetComponentInParent<TankEnemy>() != null && collider.tag != "CriticalBox" && collider.gameObject.layer != LayerMask.NameToLayer("EnemyAttack")) colliders.Remove(collider);
    }
}
