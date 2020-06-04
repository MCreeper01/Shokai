using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class GroundEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, HIT, STUNNED, DEATH }
    public State currentState = State.INITIAL;
    NavMeshAgent agent;
    NavMeshObstacle obstacle;
    //Rigidbody rb;
    Animator anim;
    [HideInInspector]
    public GameObject target;
    float empTimeStun;

    public GameObject collPoint;
    public GameObject explosionParticles;
    [HideInInspector] public float health;
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public PlayerController player;

    //GameObject player;
    [Header("Stats")]
    public float initHealth;
    public float initSpeed;
    public float initDamage;
    public float minDistAttack;
    public float maxDistAttack;    
    public float repathTime;
    public float fightRate;
    public float hitTime;
    public float deathTime;
    public int hitIncome;
    public int criticalIncome;
    public int killIncome;
    public float targetRadiusDetection;
    public float rotSpeed;

    [Header("StatIncrements")]
    public float healthInc;
    public float damageInc;
    public float maxDamage;
    public float speedInc;
    public float maxSpeed;

    // Start is called before the first frame update
    void Start()
    {
        /*
        player = GameManager.instance.player;
        //player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        rb = GetComponent<Rigidbody>();
        target = player.gameObject;
        agent.speed = speed;
        agent.stoppingDistance = minDistAttack;

        IncrementStats();*/
    }

    private void OnEnable()
    {
        player = GameManager.instance.player;
        //player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        //rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        target = player.gameObject;
        agent.speed = speed;
        agent.stoppingDistance = minDistAttack;

        IncrementStats();

        ChangeState(State.INITIAL);
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
                //transform.forward = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z);
                Vector3 newDirectionChase = Vector3.RotateTowards(transform.forward, new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z), rotSpeed * Time.deltaTime, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirectionChase);
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
                //transform.forward = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z);
                Vector3 newDirectionAttack = Vector3.RotateTowards(transform.forward, new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z), rotSpeed * Time.deltaTime, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirectionAttack);
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
                anim.SetBool("Moving", false);
                break;
            case State.ATTACK:
                anim.SetBool("Attacking", false);
                //rb.constraints = RigidbodyConstraints.None;
                obstacle.enabled = false;
                StopCoroutine("ActivateCollider");
                break;
            case State.HIT:
                //rb.constraints = RigidbodyConstraints.None;
                obstacle.enabled = false;
                break;
            case State.STUNNED:
                anim.SetBool("Stunned", false);
                //rb.constraints = RigidbodyConstraints.None;
                obstacle.enabled = false;
                break;
            case State.DEATH:                
                break;
        }

        switch (newState)
        {
            case State.CHASE:
                anim.SetBool("Moving", true);
                agent.enabled = true;                
                InvokeRepeating("GoToTarget", 0, repathTime);
                break;
            case State.ATTACK:
                anim.SetBool("Attacking", true);
                //rb.constraints = RigidbodyConstraints.FreezePosition;
                //rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                obstacle.enabled = true;
                StartCoroutine("ActivateCollider");
                break;
            case State.HIT:
                //rb.constraints = RigidbodyConstraints.FreezeAll;
                obstacle.enabled = true;
                GameManager.instance.player.IncreaseCash(hitIncome);
                Invoke("ChangeToChase", hitTime);
                break;
            case State.STUNNED:
                anim.SetBool("Stunned", true);
                //rb.constraints = RigidbodyConstraints.FreezeAll;
                obstacle.enabled = true;
                Invoke("ChangeToChase", empTimeStun);
                break;
            case State.DEATH:
                GameManager.instance.player.IncreaseCash(killIncome);
                GameManager.instance.roundController.DecreaseEnemyCount();
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
        //target = attacker;
               
    }

    void IncrementStats()
    {
        if (GameManager.instance.roundController.currentRound > 0) health = initHealth + healthInc * (GameManager.instance.roundController.currentRound - 1);
        if (GameManager.instance.roundController.currentRound > 0) speed = initSpeed + speedInc * (GameManager.instance.roundController.currentRound - 1);
        if (speed > maxSpeed) speed = maxSpeed;
        if (GameManager.instance.roundController.currentRound > 0) damage = initDamage + damageInc * (GameManager.instance.roundController.currentRound - 1);
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
        target = FindInstanceWithinRadius(gameObject, "Player", "AirTurret", "GroundTurret", targetRadiusDetection);
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

    //void OnCollisionEnter (Collision col)
    //{
    //    Debug.Log(col.gameObject.name);
    //}
}
