using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, AVOID1, AVOID2, GO_UP, ATTACK, GO_BACK, HIT, DEATH }
    public State currentState = State.INITIAL;

    NavMeshAgent agent;
    [HideInInspector] public PlayerController player;
    public GameObject bullet;
    public Transform cannon;
    public LayerMask mask;
    [Header("Stats")]
    public float health;
    public float minDistAttack;
    public float maxDistAttack;
    public float goBackDist;
    public float speed;
    //public float rayAngle;
    public GameObject[] rayPoints;

    Vector3 direction;
    float elapsedTime = 0;

    Ray[] rays;
    Ray playerRay;
    RaycastHit rayHit;
    NavMeshHit hit;
    float height;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;

        player = GameManager.instance.player;

        rays = new Ray[3];
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.INITIAL:
                elapsedTime = 5.0f;
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
                transform.LookAt(new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z));
                //AvoidObstacles();
                if (PlayerHit())
                {
                    if (rayHit.distance < 3)
                    {
                        ChangeState(State.AVOID1);
                    }
                    else if (elapsedTime >= 5.0f)
                    {
                        ChangeState(State.GO_UP);
                    }
                }
                //height = Mathf.Lerp(transform.position.y, player.position.y, Mathf.Clamp01(DistanceToTarget(gameObject, player.gameObject)));
                direction = (new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z) - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
                elapsedTime += Time.deltaTime;
                break;
            case State.AVOID1:
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                transform.LookAt(new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z));
                /*
                if (transform.position.y <= player.position.y)
                {
                    agent.baseOffset = Mathf.Lerp(transform.position.y, player.position.y + Random.Range(10, 25), 0.1f * Time.deltaTime);
                }   */             
                //AvoidObstacles2();
                if (!PlayerHit())
                {
                    ChangeState(State.CHASE);
                }
                
                break;
            case State.AVOID2:
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                transform.LookAt(new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z));
                AvoidObstacles2();
                transform.position += direction * speed * Time.deltaTime;
                break;
            case State.GO_UP:
                if (!PlayerHit())
                {
                    ChangeState(State.CHASE);
                }
                if (elapsedTime >= 2.0f)
                {
                    ChangeState(State.CHASE);
                }
                transform.position += direction * speed * Time.deltaTime;
                elapsedTime += Time.deltaTime;
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
                }
                if (transform.position.y < player.transform.position.y)
                {
                    transform.position += direction * speed * Time.deltaTime;
                }
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
                Destroy(gameObject);//, 1f);
                break;
        }
    }

    void ChangeState(State newState)
    {
        switch (currentState)
        {
            case State.CHASE:
                break;
            case State.AVOID1:
                Invoke("StopPathfinding", 0.5f);
                break;
            case State.AVOID2:
                break;
            case State.GO_UP:
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
                elapsedTime = 0;
                direction = (new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z) - transform.position).normalized;
                break;
            case State.AVOID1:
                agent.enabled = true;
                agent.destination = new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z);
                agent.baseOffset = transform.position.y;
                agent.isStopped = false;
                break;
            case State.AVOID2:
                direction = new Vector3(direction.x, 0, direction.z);
                break;
            case State.GO_UP:
                elapsedTime = 0;
                direction = direction + Vector3.up * 10;
                break;
            case State.ATTACK:
                direction = Vector3.up;
                InvokeRepeating("InstanceBullet", 0, 0.5f);
                break;
            case State.GO_BACK:
                direction = -transform.forward * 2;
                break;
            case State.HIT:
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

    void AvoidObstacles()
    {
        rays[0].origin = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        rays[1].origin = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        rays[2].origin = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        rays[0].direction = rayPoints[0].transform.position - rays[0].origin;
        rays[1].direction = rayPoints[1].transform.position - rays[1].origin;
        rays[2].direction = rayPoints[2].transform.position - rays[2].origin;
        RaycastHit hit;        
        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], out hit, 3, mask.value))
            {
                if (hit.normal.normalized == new Vector3(0, 1, 0))
                {
                    ChangeState(State.AVOID2);
                    break;
                }
                else
                {
                    Debug.Log(hit.transform.gameObject);
                    ChangeState(State.AVOID1);
                    break;
                }
            }
            Debug.DrawRay(rays[i].origin, rays[i].direction * 3, Color.red);
        }
    }
    void AvoidObstacles2()
    {
        rays[0].origin = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        rays[1].origin = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        rays[2].origin = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        rays[0].direction = rayPoints[0].transform.position - rays[0].origin;
        rays[1].direction = rayPoints[1].transform.position - rays[1].origin;
        rays[2].direction = rayPoints[2].transform.position - rays[2].origin;
        RaycastHit hit;
        bool hasHitted = false;

        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], out hit, 4, mask.value))
            {
                hasHitted = true;
                break;
            }
            Debug.DrawRay(rays[i].origin, rays[i].direction * 4, Color.red);
        }

        if (!hasHitted)
        {
            ChangeState(State.CHASE);
        }
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

    void InstanceBullet()
    {
        GameObject b;
        b = Instantiate(bullet, cannon.position, Quaternion.identity);
        b.transform.forward = player.transform.position - transform.position;
    }

    void StopPathfinding()
    {
        agent.enabled = false;
    }
}
