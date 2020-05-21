using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class TankEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, HIT, STUNNED, DEATH }
    public State currentState = State.INITIAL;
    NavMeshAgent agent;
    NavMeshObstacle obstacle;
    Rigidbody rb;
    PlayerController player;
    //GameObject player;
    public GameObject bullet;
    public Transform[] Cannons;
    public Transform[] Arms;
    bool rightCannon = true;
    float elapsedTime = 0;
    float empTimeStun;
    [HideInInspector]
    public GameObject target;

    [HideInInspector] public bool hittedByAR;

    [Header("Stats")]
    public float health;
    public float damage;
    public float minDistAttack;
    public float maxDistAttack;
    public float speed;
    public float fireRate;
    public float repathTime;
    public float hitTime;
    public int hitIncome;
    public int punishHitIncome;
    public int criticalIncome;
    public int killIncome;
    public float bulletImpulse;
    public float targetRadiusDetection;

    [Header("StatIncrements")]
    public float healthInc;
    public float damageInc;
    public float maxDamage;
    public float speedInc;
    public float maxSpeed;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.player;
        //player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        agent.speed = speed;
        agent.stoppingDistance = minDistAttack;

        IncrementStats();
    }

    // Update is called once per frame
    void Update()
    {
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
                    ChangeState(State.ATTACK);
                    break;
                }
                transform.forward = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z);
                break;
            case State.ATTACK:
                if (target == null)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, target) >= maxDistAttack * maxDistAttack)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                transform.forward = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z);
                Arms[0].forward = new Vector3(transform.forward.x, (player.transform.position.y - Arms[0].position.y) + 1f, transform.forward.z);
                Arms[1].forward = new Vector3(transform.forward.x, (player.transform.position.y - Arms[1].position.y) + 1f, transform.forward.z);
                Debug.DrawRay(Arms[0].position, Arms[0].forward, Color.red);
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
                CancelInvoke("GoToTarget");
                agent.enabled = false;
                break;
            case State.ATTACK:
                rb.constraints = RigidbodyConstraints.None;
                obstacle.enabled = false;
                CancelInvoke("InstanceBullet");
                break;
            case State.HIT:
                rb.constraints = RigidbodyConstraints.None;
                obstacle.enabled = false;
                break;
            case State.STUNNED:
                rb.constraints = RigidbodyConstraints.None;
                obstacle.enabled = false;
                break;
            case State.DEATH:
                break;
        }

        switch (newState)
        {
            case State.CHASE:
                agent.enabled = true;                
                InvokeRepeating("GoToTarget", 0, repathTime);
                break;
            case State.ATTACK:
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePosition;
                obstacle.enabled = true;
                InvokeRepeating("InstanceBullet", 0, fireRate);
                break;
            case State.HIT:
                rb.constraints = RigidbodyConstraints.FreezeAll;
                obstacle.enabled = true;
                if (hittedByAR) GameManager.instance.player.IncreaseCash(punishHitIncome);
                else GameManager.instance.player.IncreaseCash(hitIncome);
                Invoke("ChangeToChase", hitTime);
                break;
            case State.STUNNED:
                rb.constraints = RigidbodyConstraints.FreezeAll;
                obstacle.enabled = true;
                Invoke("ChangeToChase", empTimeStun);
                break;
            case State.DEATH:
                GameManager.instance.player.IncreaseCash(killIncome);
                GameManager.instance.roundController.DecreaseEnemyCount();
                Destroy(gameObject);
                break;
        }

        currentState = newState;
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
        /*Collider[] colliders = Physics.OverlapSphere(transform.position, 5);
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetType() == typeof(GroundEnemy))
                {
                    GroundEnemy ge = colliders[i].gameObject.GetComponent<GroundEnemy>();
                    ge.target = attacker;
                }
                else if (colliders[i].GetType() == typeof(Enemy3))
                {
                    Enemy3 te = colliders[i].gameObject.GetComponent<Enemy3>();
                    te.target = attacker;
                }
            }
        }*/
    }

    void IncrementStats()
    {
        health += healthInc * (GameManager.instance.roundController.currentRound - 1);
        speed += speedInc * (GameManager.instance.roundController.currentRound - 1);
        if (speed > maxSpeed) speed = maxSpeed;
        damage += damageInc * (GameManager.instance.roundController.currentRound - 1);
        if (damage > maxDamage) damage = maxDamage;
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
        agent.destination = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
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
        GameObject b;
        if (rightCannon)
        {
            b = Instantiate(bullet, Cannons[0].position, Quaternion.identity);
        }
        else
        {
            b = Instantiate(bullet, Cannons[1].position, Quaternion.identity);
        }
        
        b.GetComponent<Rigidbody>().AddForce(Arms[0].forward * bulletImpulse, ForceMode.Impulse);
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
