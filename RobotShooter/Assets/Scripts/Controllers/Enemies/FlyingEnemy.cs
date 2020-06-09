using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class FlyingEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, GO_BACK, HIT, STUNNED, DEATH }
    public State currentState = State.INITIAL;

    [Header("General")]
    public GameObject bullet;
    public GameObject explosionParticles;
    public Transform cannon;
    public LayerMask mask;
    public float minHeightFly;

    //GameObject player;
    [HideInInspector] public PlayerController player;
    [HideInInspector] public GameObject target;
    [HideInInspector] public float health;
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;

    Animator anim;

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
    Path3D p;
    Pathfinder3D pathfinder;


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
    }

    private void OnEnable()
    {
        player = GameManager.instance.player;
        //player = GameObject.FindGameObjectWithTag("Player");

        anim = GetComponent<Animator>();
        //pathfinder = new Pathfinder3D();
        //pathfinder.wayPointReachedRadius = Random.Range(0.2f, 1.0f);

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
                if (transform.position.y > minHeightFly) transform.position += direction * speed * Time.deltaTime;
                else transform.position += new Vector3(direction.x, transform.position.y, direction.z) * speed * Time.deltaTime;
                //if (pathfinder.pathFound)
                //{
                //    for (int i = 0; i < p.Path.Count - 1; i++)
                //    {
                //        Debug.DrawLine(p.Path[i].position, p.Path[i + 1].position);
                //    }

                //    if (pathfinder.currentWayPointIndex < p.Path.Count)
                //    {
                //        direction = (p.Path[pathfinder.currentWayPointIndex].position - transform.position).normalized;
                //        transform.position += direction * speed * Time.deltaTime;
                //        //transform.Translate((p.Path[pathfinder.currentWayPointIndex].position - transform.position) * Time.deltaTime);
                //        //transform.position = Vector3.MoveTowards(transform.position, p.Path[pathfinder.currentWayPointIndex].position, 0.1f);

                //        if (DistanceToTargetSquaredPlus(transform.position, p.Path[pathfinder.currentWayPointIndex].position) <= pathfinder.wayPointReachedRadius * pathfinder.wayPointReachedRadius)
                //        {
                //            pathfinder.currentWayPointIndex++;
                //        }
                //    }
                //}

                transform.LookAt(new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z));
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
                transform.LookAt(new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z));
                break;
            case State.GO_BACK:                
                if (DistanceToTargetSquared(gameObject, target) >= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                }
                transform.LookAt(new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z));
                if (transform.position.y > minHeightFly) transform.position += direction * speed * Time.deltaTime;
                else transform.position += new Vector3(direction.x, transform.position.y, direction.z) * speed * Time.deltaTime;
                break;
            case State.HIT:
                transform.LookAt(new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z));
                if (health <= 0) ChangeState(State.DEATH);
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
                break;
            case State.ATTACK:
                CancelInvoke("InstanceBullet");
                break;
            case State.GO_BACK:
                break;
            case State.HIT:
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
                //if (ProvisionalManager.Instance.gm.Graph.Count > 0)
                //{
                //    InvokeRepeating("GoToTarget", 0, repathTime);
                //}
                InvokeRepeating("GoToTarget", 0, repathTime);
                break;
            case State.ATTACK:
                InvokeRepeating("InstanceBullet", 0, fireRate);
                break;
            case State.GO_BACK:
                direction = -transform.forward * 2;
                break;
            case State.HIT:
                GameManager.instance.player.IncreaseCash(hitIncome);
                Invoke("ChangeToChase", hitTime);
                break;
            case State.STUNNED:
                anim.SetBool("Stunned", true);
                Invoke("ChangeToChase", empTimeStun);
                break;
            case State.DEATH:
                CancelInvoke("InstanceBullet");
                GameManager.instance.player.IncreaseCash(killIncome);
                GameManager.instance.roundController.DecreaseEnemyCount();
                AudioManager.instance.PlayOneShotSound("DeadExplosion", transform);
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
        if (health <= 0) ChangeState(State.DEATH);    
        else ChangeState(State.HIT);        
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
        //target = player.gameObject;
        //pathfinder.currentWayPointIndex = 0;
        //pathfinder.goal = new Vector3(target.transform.position.x, target.transform.position.y + Random.Range(1, 10), target.transform.position.z); ;
        //p = pathfinder.AStar(gameObject);
        direction = (target.transform.position + new Vector3(0, Random.Range(1, 10), 0) - transform.position).normalized;
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
    /*
    bool PlayerHit()
    {
        playerRay.origin = transform.position;
        playerRay.direction = player.transform.position - transform.position;
        if (Physics.Raycast(playerRay, out rayHit, 50, mask.value))
        {
            return true;
        }
        Debug.DrawRay(playerRay.origin, playerRay.direction * 50, Color.blue);
        return false;
    }*/

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
            b.transform.forward = player.transform.position - transform.position;
            b.GetComponent<EnemyBullet>().damage = damage;
            b.SetActive(true);
            AudioManager.instance.PlayOneShotSound("ShootEnergyBall", transform.position);
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
}
