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
    Rigidbody rb;
    [HideInInspector]
    public GameObject target;
    float empTimeStun;

    public GameObject collPoint;
    [HideInInspector] public PlayerController player;
    //GameObject player;
    [Header("Stats")]
    public float health;
    public float minDistAttack;
    public float maxDistAttack;
    public float damage;
    public float speed;
    public float repathTime;
    public float fightRate;
    public float hitTime;
    public int hitIncome;
    public int criticalIncome;
    public int killIncome;

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
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        rb = GetComponent<Rigidbody>();
        target = player.gameObject;
        agent.speed = speed;
        agent.stoppingDistance = minDistAttack;

        IncrementStats();
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z);

        switch (currentState)
        {
            case State.INITIAL:
                ChangeState(State.CHASE);
                break;
            case State.CHASE:
                if (DistanceToTargetSquared(gameObject, target) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                break;
            case State.ATTACK:
                if (target == null)
                {
                    target = player.gameObject;
                    ChangeState(State.CHASE);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, target) >= maxDistAttack * maxDistAttack)
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
                StopCoroutine("ActivateCollider");
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
                rb.constraints = RigidbodyConstraints.FreezePosition;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                obstacle.enabled = true;
                StartCoroutine("ActivateCollider");
                break;
            case State.HIT:
                rb.constraints = RigidbodyConstraints.FreezeAll;
                obstacle.enabled = true;
                GameManager.instance.player.IncreaseCash(hitIncome);
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
        } */       
    }

    void IncrementStats()
    {
        health += healthInc * (GameManager.instance.roundController.currentRound - 1);
        speed += speedInc * (GameManager.instance.roundController.currentRound - 1);
        if (speed > maxSpeed) speed = maxSpeed;
        damage += damageInc * (GameManager.instance.roundController.currentRound - 1);
        if (damage > maxDamage) damage = maxDamage;
    }

    public void ActivateStun(float time)
    {
        empTimeStun = time;
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
        agent.destination = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
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
