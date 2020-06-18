using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.Audio;

public class FlyingEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, GO_BACK, STUNNED, DEATH }
    public State currentState = State.INITIAL;

    [Header("General")]
    public GameObject bullet;
    public GameObject explosionParticles;
    public Transform cannon;
    public LayerMask nearObjectsMask;
    public float minHeightFly;

    //GameObject player;
    [HideInInspector] public PlayerController player;
    [HideInInspector] public GameObject target;
    [HideInInspector] public float health;
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;

    Animator anim;
    Rigidbody rb;
    Collider[] nearObjects;
    public AudioSource source;
    public AudioSource source2;

    [Header("Stats")]
    public float initHealth;
    public float initSpeed;
    public float initDamage;
    public float minDistAttack;
    public float maxDistAttack;
    public float goBackDist;
    public float fireRate;
    public float repathTime;
    public int hitIncome;
    public int criticalIncome;
    public int killIncome;
    public float hitTime;
    public float deathTime;
    public float targetRadiusDetection;
    public float rotSpeedCharacter;

    [Header("StatIncrements")]
    public float healthInc;
    public float damageInc;
    public float maxDamage;
    public float speedInc;
    public float maxSpeed;

    [HideInInspector] public Vector3 direction;
    float elapsedTime = 0;

    //Ray[] rays;
    //Ray playerRay;
    //RaycastHit rayHit;
    //NavMeshHit hit;
    //float height;
    float empTimeStun;


    //Pathfinding 3D
    //Path3D p;
    //Pathfinder3D pathfinder;


    // Start is called before the first frame update
    void Start()
    {/*
        player = GameManager.instance.player;
        //player = GameObject.FindGameObjectWithTag("Player");

        rays = new Ray[3];

        rb = GetComponent<Rigidbody>();
        pathfinder = new Pathfinder3D();
        pathfinder.wayPointReachedRadius = Random.Range(0.2f, 1.0f);

        //IncrementStats();*/
        AudioManager.instance.unitySources.Add(source2);
    }

    private void OnEnable()
    {
        player = GameManager.instance.player;
        //player = GameObject.FindGameObjectWithTag("Player");

        anim = GetComponent<Animator>();
        //source = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        IncrementStats();

        ChangeState(State.INITIAL);
    }

    private void OnDisable()
    {
        if (GameManager.instance.roundController.currentState == RoundController.State.FIGHT ||
            GameManager.instance.roundController.currentState == RoundController.State.SPAWN)
            GameManager.instance.roundController.DecreaseEnemyCount();
    }

    // Update is called once per frame
    void Update()
    {
        if (!source2.isPlaying && !GameManager.instance.uiController.paused)
        {
            source2.Play();
        }
        if (target != null)
        {
            transform.forward = Vector3.Lerp(transform.forward, target.transform.position - transform.position, Time.deltaTime * rotSpeedCharacter);
        } 
        switch (currentState)
        {
            case State.INITIAL:
                ChangeState(State.CHASE);
                break;
            case State.CHASE:
                if (target == null)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, target) <= goBackDist * goBackDist)
                {
                    ChangeState(State.GO_BACK);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, target) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                transform.position += direction * speed * Time.deltaTime;

                //Vector3 newDirectionChase = Vector3.RotateTowards(transform.forward, target.transform.position - transform.position, rotSpeedCharacter * Time.deltaTime, 0.0f);
                //transform.rotation = Quaternion.LookRotation(newDirectionChase);
                //transform.LookAt(new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z));
                break;
            case State.ATTACK:
                if (target == null)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, target) <= goBackDist * goBackDist)
                {
                    ChangeState(State.GO_BACK);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, target) >= maxDistAttack * maxDistAttack)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                //Vector3 newDirectionAttack = Vector3.RotateTowards(transform.forward, target.transform.position - transform.position, rotSpeedCharacter * Time.deltaTime, 0.0f);
                //transform.rotation = Quaternion.LookRotation(newDirectionAttack);
                //transform.LookAt(new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z));
                break;
            case State.GO_BACK:                
                if (DistanceToTargetSquared(gameObject, target) >= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                }
                //Vector3 newDirectionGoBack = Vector3.RotateTowards(transform.forward, target.transform.position - transform.position, rotSpeedCharacter * Time.deltaTime, 0.0f);
                //transform.rotation = Quaternion.LookRotation(newDirectionGoBack);
                //transform.LookAt(new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z));
                transform.position += direction * speed * Time.deltaTime;
                break;
            case State.DEATH:                
                break;
        }

        rb.velocity = Vector3.zero;
    }

    void ChangeState(State newState)
    {
        switch (currentState)
        {
            case State.CHASE:
                CancelInvoke("GoToTarget");
                break;
            case State.ATTACK:
                CancelInvoke("InstanceBullet");
                break;
            case State.GO_BACK:
                break;
            case State.STUNNED:
                anim.SetBool("Stunned", false);
                break;
            case State.DEATH:
                break;
        }        

        switch (newState)
        {
            case State.CHASE:
                elapsedTime = 0;
                InvokeRepeating("GoToTarget", 0, repathTime);
                break;
            case State.ATTACK:
                InvokeRepeating("InstanceBullet", 0, fireRate);
                break;
            case State.GO_BACK:
                direction = -transform.forward * 2;
                break;
            case State.STUNNED:
                anim.SetBool("Stunned", true);
                Invoke("ChangeToChase", empTimeStun);
                break;
            case State.DEATH:
                CancelInvoke("InstanceBullet");
                GameManager.instance.player.IncreaseCash(killIncome);
                //GameManager.instance.roundController.DecreaseEnemyCount();
                AudioManager.instance.PlayOneShotSound("DeadExplosion", transform.position);
                Instantiate(explosionParticles, transform.position, transform.rotation);
                Invoke("DisableEnemy", deathTime);
                break;
        }

        currentState = newState;
    }

    void DisableEnemy()
    {        
        gameObject.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        if (currentState == State.DEATH) return;
        health -= damage;
        GameManager.instance.player.IncreaseCash(hitIncome);
        if (health <= 0) ChangeState(State.DEATH);         
    }

    void IncrementStats()
    {
        if (GameManager.instance.roundController.currentRound > 0) health = initHealth + healthInc * (GameManager.instance.roundController.currentRound - 1);
        if (GameManager.instance.roundController.currentRound > 0) speed = initSpeed + speedInc * (GameManager.instance.roundController.currentRound - 1);
        if (speed > maxSpeed) speed = maxSpeed;
        if (GameManager.instance.roundController.currentRound > 0) damage = initDamage + damageInc * (GameManager.instance.roundController.currentRound - 1);
        if (damage > maxDamage) damage = maxDamage;
    }

    void GoToTarget()
    {
        target = FindInstanceWithinRadius(gameObject, "Player", "AirTurret", "GroundTurret", targetRadiusDetection);

        if (target.transform.position.y <= minHeightFly)
        {
            direction = (new Vector3(target.transform.position.x, minHeightFly, target.transform.position.z) - transform.position).normalized;
        }
        else
        {
            direction = (target.transform.position - transform.position).normalized;
        }
    }

    public void WhatHitSound(string soundName)
    {
        AudioManager.instance.PlayOneShotSound(soundName, transform.position);
    }

    float DistanceToTarget(GameObject me, GameObject target)
    {
        return (target.transform.position - me.transform.position).magnitude;
    }

    float DistanceToTargetSquared(GameObject me, GameObject target)
    {
        return (target.transform.position - me.transform.position).sqrMagnitude;
    }

    float DistanceToTargetSquaredPlus(Vector3 me, Vector3 target)
    {
        return (target - me).sqrMagnitude;
    }

    void ChangeToChase()
    {
        ChangeState(State.CHASE);
    }

    public void ActivateStun(float time)
    {
        empTimeStun = time;
        ChangeState(State.STUNNED);
    }

    void InstanceBullet()
    {
        if (gameObject.activeInHierarchy)
        {
            GameObject b;
            b = GameManager.instance.objectPoolerManager.airEnemyBulletOP.GetPooledObject();
            b.transform.position = cannon.position;
            b.transform.forward = target.transform.position - transform.position;
            b.GetComponent<EnemyBullet>().damage = damage;
            b.SetActive(true);
            //AudioManager.instance.PlayOneShotSound("ShootEnergyBall", transform.position);
            source.clip = AudioManager.instance.clips[1];
            source.volume *= AudioManager.instance.fXVolume * AudioManager.instance.masterVolume;
            source.Play();
        }
    }

    public static GameObject FindInstanceWithinRadius(GameObject me, string tag, string tag2, string tag3, float radius)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag).Concat(GameObject.FindGameObjectsWithTag(tag2)).Concat(GameObject.FindGameObjectsWithTag(tag3)).ToArray();

        if (targets.Length == 0) return null;

        float dist = 0;
        GameObject closest = targets[0];
        float minDistance = (closest.transform.position - me.transform.position).magnitude;

        for (int i = 1; i < targets.Length; i++)
        {
            dist = (targets[i].transform.position - me.transform.position).magnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = targets[i];
            }
        }

        if (minDistance < radius) return closest;
        else return null;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "HeightLimit") TakeDamage(0148383);
    }
}
