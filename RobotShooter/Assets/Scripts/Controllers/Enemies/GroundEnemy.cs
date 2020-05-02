using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, HIT, DEATH }
    public State currentState = State.INITIAL;
    NavMeshAgent agent;

    public GameObject collPoint;
    [HideInInspector] public PlayerController player;
    [Header("Stats")]
    public float health;
    public float minDistAttack;
    public float maxDistAttack;
    public float damage;
    public float speed;
    public float repathTime;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.player;
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
                agent.isStopped = true;
                break;
            case State.ATTACK:
                StopCoroutine("ActivateCollider");
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
                agent.isStopped = true;
                StartCoroutine("ActivateCollider");
                break;
            case State.HIT:
                Invoke("ChangeToAttack", 0.5f);
                break;
            case State.DEATH:
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

    void ChangeToAttack()
    {
        ChangeState(State.ATTACK);
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            player.TakeDamage(damage, 0);
        }        
    }

    IEnumerator ActivateCollider()
    {
        while (true)
        {            
            collPoint.GetComponent<BoxCollider>().enabled = true;
            yield return new WaitForSeconds(0.1f);
            collPoint.GetComponent<BoxCollider>().enabled = false;
            yield return new WaitForSeconds(1.0f);
        }
        
    }
}
