using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, GO_BACK, HIT, STUNNED, DEATH }
    public State currentState = State.INITIAL;

    [HideInInspector] public PlayerController player;
    //GameObject player;
    [HideInInspector]
    public Vector3 target;
    public GameObject bullet;
    public Transform cannon;
    public LayerMask mask;
    [Header("Stats")]
    public float health;
    public float minDistAttack;
    public float maxDistAttack;
    public float goBackDist;
    public float speed;
    public float fireRate;
    public float repathTime;
    public float empTimeStun;
    public int hitIncome;
    public int killIncome;
    public float hitTime;

    public GameObject[] rayPoints;

    public Vector3 direction;
    float elapsedTime = 0;

    Ray[] rays;
    Ray playerRay;
    RaycastHit rayHit;
    NavMeshHit hit;
    float height;


    //Pathfinding 3D
    Path3D p;
    Pathfinder3D pathfinder;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.player;
        //player = GameObject.FindGameObjectWithTag("Player");

        rays = new Ray[3];

        pathfinder = new Pathfinder3D();
        pathfinder.wayPointReachedRadius = Random.Range(0.2f, 1.0f);
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
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= goBackDist * goBackDist)
                {
                    ChangeState(State.GO_BACK);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                if (pathfinder.pathFound)
                {
                    for (int i = 0; i < p.Path.Count - 1; i++)
                    {
                        Debug.DrawLine(p.Path[i].position, p.Path[i + 1].position);
                    }

                    if (pathfinder.currentWayPointIndex < p.Path.Count)
                    {
                        direction = (p.Path[pathfinder.currentWayPointIndex].position - transform.position).normalized;
                        transform.position += direction * speed * Time.deltaTime;
                        //transform.Translate((p.Path[pathfinder.currentWayPointIndex].position - transform.position) * Time.deltaTime);
                        //transform.position = Vector3.MoveTowards(transform.position, p.Path[pathfinder.currentWayPointIndex].position, 0.1f);

                        if (DistanceToTargetSquaredPlus(transform.position, p.Path[pathfinder.currentWayPointIndex].position) <= pathfinder.wayPointReachedRadius * pathfinder.wayPointReachedRadius)
                        {
                            pathfinder.currentWayPointIndex++;
                        }
                    }
                }
                transform.LookAt(new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z));
                break;
            case State.ATTACK:
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= goBackDist * goBackDist)
                {
                    ChangeState(State.GO_BACK);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, player.gameObject) >= maxDistAttack * maxDistAttack)
                {
                    ChangeState(State.CHASE);
                    break;
                }/*
                if (transform.position.y < player.transform.position.y)
                {
                    transform.position += direction * speed * Time.deltaTime;
                }*/
                transform.LookAt(new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z));
                break;
            case State.GO_BACK:                
                if (DistanceToTargetSquared(gameObject, player.gameObject) >= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                }
                transform.LookAt(new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z));
                transform.position += direction * speed * Time.deltaTime;
                break;
            case State.HIT:
                transform.LookAt(new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z));
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
            case State.DEATH:
                break;
        }

        switch (newState)
        {
            case State.CHASE:
                if (ProvisionalManager.Instance.currentGraph.Graph.Count > 0)
                {
                    InvokeRepeating("GoToTarget", 0, repathTime);
                }
                break;
            case State.ATTACK:
                InvokeRepeating("InstanceBullet", 0, fireRate);
                break;
            case State.GO_BACK:
                direction = -transform.forward * 2;
                break;
            case State.HIT:
                GameManager.instance.player.IncreaseCash(hitIncome);
                ChangeState(State.CHASE);
                break;
            case State.STUNNED:
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

    public void TakeDamage(float damage)
    {
        if (currentState == State.DEATH) return;
        health -= damage;
        if (health <= 0) ChangeState(State.DEATH);        
        else ChangeState(State.HIT);        
    }

    void GoToTarget()
    {
        pathfinder.currentWayPointIndex = 0;
        pathfinder.goal = new Vector3(player.transform.position.x, player.transform.position.y + Random.Range(1, 10), player.transform.position.z); ;
        p = pathfinder.AStar(gameObject);
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
    }

    void ChangeToChase()
    {
        ChangeState(State.CHASE);
    }

    void InstanceBullet()
    {
        GameObject b;
        b = Instantiate(bullet, cannon.position, Quaternion.identity);
        b.transform.forward = player.transform.position - transform.position;
    }
}
