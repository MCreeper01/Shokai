using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, HIT, DEATH }
    public State currentState = State.INITIAL;
    NavMeshAgent agent;

    [HideInInspector] public PlayerController player;
    [Header("Stats")]
    public float health;
    public float minDistAttack;
    public float maxDistAttack;
    public float healthLostByHit;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.player;
        agent = GetComponent<NavMeshAgent>();
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
                break;
            case State.HIT:
                break;
            case State.DEATH:
                break;
        }

        switch (newState)
        {
            case State.CHASE:
                InvokeRepeating("GoToTarget", 0, 1f);                
                agent.isStopped = false;
                break;
            case State.ATTACK:
                break;
            case State.HIT:
                health -= healthLostByHit;
                Invoke("ChangeTo", 1.0f);
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

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "PlayerBullet")//tag de la bullet del player
        {
            ChangeState(State.HIT);
        }
    }

    void ChangeTo()
    {
        ChangeState(State.ATTACK);
    }
}
