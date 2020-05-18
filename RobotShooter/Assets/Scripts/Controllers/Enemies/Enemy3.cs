using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy3 : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, HIT, STUNNED, DEATH }
    public State currentState = State.INITIAL;
    NavMeshAgent agent;
    NavMeshObstacle obstacle;
    Rigidbody rb;
    //PlayerController player;
    GameObject player;
    public GameObject bullet;
    public Transform[] Cannons;
    bool rightCannon = true;
    float elapsedTime = 0;
    [HideInInspector]
    public GameObject target;

    [Header("Stats")]
    public float health;
    public float minDistAttack;
    public float maxDistAttack;
    public float speed;
    public float fireRate;
    public float repathTime;
    public float empTimeStun;
    public float hitTime;
    public int hitIncome;
    public int killIncome;

    // Start is called before the first frame update
    void Start()
    {
        //player = GameManager.instance.player;
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        target = player;
        agent.speed = speed;
        agent.stoppingDistance = minDistAttack;
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
                Debug.DrawLine(transform.position, agent.nextPosition);
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                transform.forward = agent.velocity;
                break;
            case State.ATTACK:
                if (target == null)
                {
                    target = player;
                    ChangeState(State.CHASE);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, player.gameObject) >= maxDistAttack * maxDistAttack)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                transform.forward = new Vector3(player.transform.position.x - transform.position.x, 0, player.transform.position.z - transform.position.z);
                break;
            case State.HIT:
                if (health <= 0) ChangeState(State.DEATH);
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
                break;
            case State.STUNNED:
                rb.constraints = RigidbodyConstraints.None;
                obstacle.enabled = false;
                break;
            case State.DEATH:
                Destroy(gameObject);
                break;
        }

        switch (newState)
        {
            case State.CHASE:
                agent.enabled = true;
                InvokeRepeating("GoToTarget", 0, repathTime);
                break;
            case State.ATTACK:
                rb.constraints = RigidbodyConstraints.FreezePosition;
                obstacle.enabled = true;
                InvokeRepeating("InstanceBullet", 0, fireRate);
                break;
            case State.HIT:
                GameManager.instance.player.IncreaseCash(hitIncome);
                ChangeState(State.CHASE);
                break;
            case State.STUNNED:
                rb.constraints = RigidbodyConstraints.FreezePosition;
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
        ChangeState(State.HIT);
        health -= damage;
        /*target = attacker;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5);
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

    public void IncreaseStates(float health, float dmg)
    {

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
        agent.destination = target.transform.position;
    }

    void ChangeToChase()
    {
        ChangeState(State.CHASE);
    }

    public void ActivateStun()
    {
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
        b.transform.forward = player.transform.position - transform.position;
        b.GetComponent<Rigidbody>().AddForce(/*(new Vector3(player.transform.position.x, player.transform.position.y + 2, player.transform.position.z) - transform.position)*/ b.transform.forward * 15, ForceMode.Impulse);
        rightCannon = !rightCannon;
    }
}
