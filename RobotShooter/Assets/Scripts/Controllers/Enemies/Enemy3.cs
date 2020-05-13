using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy3 : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, HIT, DEATH }
    public State currentState = State.INITIAL;
    NavMeshAgent agent;
    //PlayerController player;
    GameObject player;
    public GameObject bullet;
    public Transform[] Cannons;
    bool rightCannon = true;
    bool canMove = false;

    [Header("Stats")]
    public float health;
    public float minDistAttack;
    public float maxDistAttack;
    public float speed;
    public float fireRate;
    public float repathTime;
    public int cashDropped;

    // Start is called before the first frame update
    void Start()
    {
        //player = GameManager.instance.player;
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
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
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                transform.forward = agent.velocity;
                break;
            case State.ATTACK:
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
            case State.DEATH:
                Destroy(gameObject);
                break;
        }
    }

    void ChangeState(State newState)
    {
        switch (currentState)
        {
            case State.CHASE:
                agent.isStopped = true;
                break;
            case State.ATTACK:
                break;
            case State.HIT:
                break;
            case State.DEATH:
                break;
        }

        switch (newState)
        {
            case State.CHASE:
                InvokeRepeating("GoToTarget", 0, repathTime);
                agent.isStopped = false;
                break;
            case State.ATTACK:
                InvokeRepeating("InstanceBullet", 0, fireRate);
                break;
            case State.HIT:
                if (canMove)
                {
                    ChangeState(State.CHASE);
                }
                else
                {
                    Invoke("ChangeToChase", 0.5f);
                }
                canMove = !canMove;
                break;
            case State.DEATH:
                break;
        }

        currentState = newState;
    }

    public void TakeDamage(float damage)
    {
        if (currentState == State.DEATH) return;
        ChangeState(State.HIT);
        health -= damage;
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
        agent.destination = player.transform.position;
    }

    void ChangeToChase()
    {
        ChangeState(State.CHASE);
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
        rightCannon = !rightCannon;
    }

    void UpdateRotation()
    {

    }
}
