using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using FMOD.Studio;

public class TankEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, HIT, STUNNED, DEATH }
    public State currentState = State.INITIAL;
    NavMeshAgent agent;
    NavMeshObstacle obstacle;
    Animator anim;
    PlayerController player;
    //GameObject player;
    public GameObject bullet;
    public GameObject explosionParticles;
    public Transform[] Cannons;
    public Transform[] Arms;
    public LayerMask mask;
    bool rightCannon = true;
    float elapsedTime = 0;
    float empTimeStun;
    [HideInInspector]
    public GameObject target;
    private bool rotating;
    private Vector3 relativePosition;
    private float rotationTime;
    private Quaternion targetRotation;
    EventInstance heavyLevitationSound;

    [HideInInspector] public float health;
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public bool hittedByAR;
    public AudioSource source;
    public AudioSource source2;

    [Header("Stats")]
    public float initHealth;
    public float initSpeed;
    public float initDamage;
    public float minDistAttack;
    public float maxDistAttack;
    public float fireRate;
    public float repathTime;
    public float hitTime;
    public float deathTime;
    public int hitIncome;
    public int punishHitIncome;
    public int criticalIncome;
    public int killIncome;
    public float targetRadiusDetection;
    public float rotSpeedCharacter;
    public float rotSpeedArms;

    [Header("StatIncrements")]
    public float healthInc;
    public float damageInc;
    public float maxDamage;
    public float speedInc;
    public float maxSpeed;

    // Start is called before the first frame update
    void Start()
    {/*
        //player = GameManager.instance.player;
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        agent.speed = speed;
        agent.stoppingDistance = minDistAttack;
        armInitForward = Arms[0].forward;

        IncrementStats();*/
        AudioManager.instance.unitySources.Add(source2);
    }

    private void OnEnable()
    {
        player = GameManager.instance.player;
        //player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        //source = GetComponent<AudioSource>();
        agent.speed = speed;
        agent.stoppingDistance = minDistAttack;

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
                if (DistanceToTargetSquared(gameObject, target) <= minDistAttack * minDistAttack)
                {
                    if (!PlayerHitLeft() && !PlayerHitRight())
                    {
                        ChangeState(State.ATTACK);
                        break;
                    }                    
                }
                //transform.forward = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z);
                Vector3 newDirectionChase = Vector3.RotateTowards(transform.forward, new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z), rotSpeedCharacter * Time.deltaTime, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirectionChase);
                Arms[0].localRotation = Quaternion.Lerp(Arms[0].localRotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * rotSpeedArms);
                Arms[1].localRotation = Quaternion.Lerp(Arms[1].localRotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * rotSpeedArms);
                break;
            case State.ATTACK:
                if (target == null)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, target) >= maxDistAttack * maxDistAttack)// || (PlayerHitLeft() && PlayerHitRight()))
                {
                    ChangeState(State.CHASE);
                    break;
                }
                //transform.forward = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z);
                Vector3 newDirectionAttack = Vector3.RotateTowards(transform.forward, new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z), rotSpeedCharacter * Time.deltaTime, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirectionAttack);

                Vector3 tPos0 = (new Vector3(target.transform.position.x, target.transform.position.y - 1, target.transform.position.z) - Arms[0].position).normalized;
                Vector3 tPos1 = (new Vector3(target.transform.position.x, target.transform.position.y - 1, target.transform.position.z) - Arms[1].position).normalized;

                if (Vector3.Dot(transform.forward, (target.transform.position - transform.position).normalized) > 0)
                {
                    if (tPos0.y >= 0)
                    {
                        Arms[0].localRotation = Quaternion.Lerp(Arms[0].localRotation, Quaternion.Euler(-Vector3.Angle(transform.forward, tPos0), 0, 0), Time.deltaTime * rotSpeedArms);
                        Arms[1].localRotation = Quaternion.Lerp(Arms[1].localRotation, Quaternion.Euler(-Vector3.Angle(transform.forward, tPos1), 0, 0), Time.deltaTime * rotSpeedArms);
                    }
                    else
                    {
                        Arms[0].localRotation = Quaternion.Lerp(Arms[0].localRotation, Quaternion.Euler(Vector3.Angle(transform.forward, tPos0), 0, 0), Time.deltaTime * rotSpeedArms);
                        Arms[1].localRotation = Quaternion.Lerp(Arms[1].localRotation, Quaternion.Euler(Vector3.Angle(transform.forward, tPos1), 0, 0), Time.deltaTime * rotSpeedArms);
                    }
                }            
                break;
            case State.HIT:
                if (health <= 0) ChangeState(State.DEATH);
                if (hittedByAR) hittedByAR = false;
                break;
            case State.STUNNED:
                break;
            case State.DEATH:                
                break;
        }
    }

    void ChangeState(State newState)
    {
        switch (currentState)
        {
            case State.CHASE:
                heavyLevitationSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                CancelInvoke("GoToTarget");
                agent.enabled = false;
                break;
            case State.ATTACK:
                obstacle.enabled = false;
                CancelInvoke("InstanceBullet");
                break;
            case State.HIT:
                obstacle.enabled = false;
                break;
            case State.STUNNED:
                anim.SetBool("Stunned", false);
                obstacle.enabled = false;
                break;
            case State.DEATH:
                //gameObject.SetActive(false);
                break;
        }

        switch (newState)
        {
            case State.CHASE:
                /*if (!AudioManager.instance.isPlaying(heavyLevitationSound))
                {
                    heavyLevitationSound = AudioManager.instance.PlayOneShotSound("HeavyLevitationSound", transform.position);
                }*/
                
                agent.enabled = true;
                InvokeRepeating("GoToTarget", 0, repathTime);
                break;
            case State.ATTACK:
                obstacle.enabled = true;
                InvokeRepeating("InstanceBullet", 0.3f, fireRate);
                break;
            case State.HIT:                
                obstacle.enabled = true;
                if (hittedByAR)
                {
                    GameManager.instance.player.IncreaseCash(punishHitIncome);
                }
                else
                {
                    GameManager.instance.player.IncreaseCash(hitIncome);
                }
                Invoke("ChangeToChase", hitTime);
                break;
            case State.STUNNED:
                anim.SetBool("Stunned", true);
                obstacle.enabled = true;
                Invoke("ChangeToChase", empTimeStun);
                break;
            case State.DEATH:
                GameManager.instance.player.IncreaseCash(killIncome);
                //GameManager.instance.roundController.DecreaseEnemyCount();
                heavyLevitationSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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

    public void TakeDamage(float damage/*, GameObject attacker*/)
    {
        if (currentState == State.DEATH) return;
        health -= damage;
        if (health <= 0) ChangeState(State.DEATH);
        else ChangeState(State.HIT);
        /*if (attacker.GetType() != typeof(PlayerController))
        {
            target = attacker;
        }   */
        
    }

    void IncrementStats()
    {
        if (GameManager.instance.roundController.currentRound > 0) health = initHealth + healthInc * (GameManager.instance.roundController.currentRound - 1);
        if (GameManager.instance.roundController.currentRound > 0) speed = initSpeed + speedInc * (GameManager.instance.roundController.currentRound - 1);
        if (speed > maxSpeed) speed = maxSpeed;
        if (GameManager.instance.roundController.currentRound > 0) damage = initDamage + damageInc * (GameManager.instance.roundController.currentRound - 1);
        if (damage > maxDamage) damage = maxDamage;
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

    void GoToTarget()
    {
        target = FindInstanceWithinRadius(gameObject, "Player", "AirTurret", "GroundTurret", targetRadiusDetection);
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.destination = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        }        
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

    bool PlayerHitLeft()
    {
        RaycastHit rayHit;
        Ray playerRay = new Ray();
        playerRay.origin = Cannons[1].position;
        playerRay.direction = player.transform.position - Cannons[1].position;
        Debug.DrawRay(playerRay.origin, playerRay.direction * 50, Color.blue);
        if (Physics.Raycast(playerRay, out rayHit, maxDistAttack + 10, mask.value))
        {
            if (rayHit.collider.gameObject == player.gameObject) return false;
            else return true;
        }
        
        return false;
    }

    bool PlayerHitRight()
    {
        RaycastHit rayHit;
        Ray playerRay = new Ray();
        playerRay.origin = Cannons[0].position;
        playerRay.direction = player.transform.position - Cannons[0].position;
        Debug.DrawRay(playerRay.origin, playerRay.direction * 50, Color.blue);
        if (Physics.Raycast(playerRay, out rayHit, maxDistAttack + 10, mask.value))
        {
            if (rayHit.collider.gameObject == player.gameObject) return false;
            else return true;
        }        
        return false;
    }

    void InstanceBullet()
    {
        if (PlayerHitLeft() && PlayerHitRight())
        {
            ChangeState(State.CHASE);
            return;
        } 
        GameObject b;
        if (rightCannon)
        {
            b = GameManager.instance.objectPoolerManager.tankEnemyBulletOP.GetPooledObject();
            b.transform.position = Cannons[0].position;
            b.transform.forward = Arms[0].forward;
            b.SetActive(true);
        }
        else
        {
            b = GameManager.instance.objectPoolerManager.tankEnemyBulletOP.GetPooledObject();
            b.transform.position = Cannons[1].position;
            b.transform.forward = Arms[1].forward;
            b.SetActive(true);
        }
        AudioManager.instance.PlayOneShotSound("PlasmaBallShot", b.transform.position);
        b.GetComponent<TankBullet>().damage = damage;
        rightCannon = !rightCannon;
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
}
