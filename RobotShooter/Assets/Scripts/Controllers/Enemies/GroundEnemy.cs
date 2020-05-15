using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, HIT, STUNNED, DEATH }
    public State currentState = State.INITIAL;
    NavMeshAgent agent;
    NavMeshObstacle obstacle;
    bool canMove = false;
    Vector3 target;

    public GameObject collPoint;
    //[HideInInspector] public PlayerController player;
    GameObject player;
    [Header("Stats")]
    public float health;
    public float minDistAttack;
    public float maxDistAttack;
    public float damage;
    public float speed;
    public float repathTime;
    public int cashDropped;
    public float fightRate;
    public float empTimeStun;

    // Start is called before the first frame update
    void Start()
    {
        //player = GameManager.instance.player;
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        agent.speed = speed;
        agent.stoppingDistance = minDistAttack;
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = new Vector3(player.transform.position.x - transform.position.x, 0, player.transform.position.z - transform.position.z);

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
                break;
            case State.ATTACK:
                if (DistanceToTargetSquared(gameObject, player.gameObject) >= maxDistAttack * maxDistAttack)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                break;
            case State.HIT:
                if (health <= 0) ChangeState(State.DEATH);
                break;
            case State.STUNNED:
                break;
            case State.DEATH:
                Destroy(gameObject);//, 1f);
                break;
        }
    }

    void ChangeState(State newState)
    {
        switch (currentState)
        {
            case State.CHASE:
                //agent.isStopped = true;
                CancelInvoke("GoToTarget");
                agent.enabled = false;
                break;
            case State.ATTACK:
                obstacle.enabled = false;
                StopCoroutine("ActivateCollider");
                break;
            case State.HIT:
                break;
            case State.STUNNED:
                obstacle.enabled = false;
                break;
            case State.DEATH:
                break;
        }

        switch (newState)
        {
            case State.CHASE:
                agent.enabled = true;
                target = player.transform.position;
                InvokeRepeating("GoToTarget", 0, repathTime);                
                //agent.isStopped = false;
                break;
            case State.ATTACK:
                //agent.isStopped = true;
                obstacle.enabled = true;
                StartCoroutine("ActivateCollider");
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
            case State.STUNNED:
                obstacle.enabled = true;
                Invoke("ChangeToChase", empTimeStun);
                break;
            case State.DEATH:
                GameManager.instance.player.IncreaseCash(cashDropped);
                GameManager.instance.roundController.DecreaseEnemyCount();
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

    public void ActivateStun()
    {
        ChangeState(State.STUNNED);
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
        agent.destination = target;
    }

    void ChangeToChase()
    {
        ChangeState(State.CHASE);
    }

    IEnumerator ActivateCollider()
    {
        while (true)
        {            
            collPoint.GetComponent<BoxCollider>().enabled = true;
            yield return new WaitForSeconds(0.1f);
            collPoint.GetComponent<BoxCollider>().enabled = false;
            yield return new WaitForSeconds(fightRate);
        }
        
    }
}
