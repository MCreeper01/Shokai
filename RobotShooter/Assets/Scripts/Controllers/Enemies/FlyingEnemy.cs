using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, AVOID1, AVOID2, ATTACK, GO_BACK, HIT, DEATH }
    public State currentState = State.INITIAL;

    NavMeshAgent agent;
    public Transform player;
    [Header("Stats")]
    public float health;
    public float minDistAttack;
    public float maxDistAttack;
    public float healthLostByHit;
    public float speed;
    //public float rayAngle;
    public GameObject[] rayPoints;

    Vector3 direction;
    float elapsedTime = 0;

    Ray[] rays;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;

        rays = new Ray[3];
        
        rays[0].direction = transform.forward;        
        rays[1].direction = transform.forward;        
        rays[2].direction = transform.forward;
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
                if (health <= 0)
                {
                    ChangeState(State.DEATH);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                transform.LookAt(new Vector3(player.position.x, player.position.y + 1, player.position.z));
                AvoidObstacles();               
                transform.position += direction * speed * Time.deltaTime;
                break;
            case State.AVOID1:
                if (health <= 0)
                {
                    ChangeState(State.DEATH);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                transform.LookAt(new Vector3(player.position.x, player.position.y + 1, player.position.z));
                AvoidObstacles2();                
                break;
            case State.AVOID2:
                if (health <= 0)
                {
                    ChangeState(State.DEATH);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, player.gameObject) <= minDistAttack * minDistAttack)
                {
                    ChangeState(State.ATTACK);
                    break;
                }
                transform.LookAt(new Vector3(player.position.x, player.position.y + 1, player.position.z));
                AvoidObstacles2();
                transform.position += direction * speed * Time.deltaTime;
                break;
            case State.ATTACK:
                if (health <= 0)
                {
                    ChangeState(State.DEATH);
                    break;
                }
                if (DistanceToTargetSquared(gameObject, player.gameObject) >= maxDistAttack * maxDistAttack)
                {
                    ChangeState(State.CHASE);
                    break;
                }
                transform.LookAt(new Vector3(player.position.x, player.position.y + 1, player.position.z));
                break;
            case State.GO_BACK:
                if (health <= 0)
                {
                    ChangeState(State.DEATH);
                    break;
                }
                transform.LookAt(new Vector3(player.position.x, player.position.y + 1, player.position.z));
                break;
            case State.HIT:
                transform.LookAt(new Vector3(player.position.x, player.position.y + 1, player.position.z));
                break;
            case State.DEATH:
                Destroy(gameObject, 1f);
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
                agent.isStopped = true;
                agent.enabled = false;
                break;
            case State.AVOID2:
                break;
            case State.ATTACK:
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
                Debug.Log("Chase");
                direction = (new Vector3(player.position.x, player.position.y + 1, player.position.z) - transform.position).normalized;
                break;
            case State.AVOID1:
                Debug.Log("Avoid1");
                agent.enabled = true;
                agent.destination = new Vector3(player.position.x, player.position.y + 1, player.position.z);
                agent.baseOffset = transform.position.y;
                agent.isStopped = false;
                break;
            case State.AVOID2:
                Debug.Log("Avoid2");
                direction = new Vector3(direction.x, 0, direction.z);
                break;
            case State.ATTACK:
                Debug.Log("Attack");
                break;
            case State.GO_BACK:
                break;
            case State.HIT:
                break;
            case State.DEATH:
                break;
        }

        currentState = newState;
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
            if (Physics.Raycast(rays[i], out hit, 3))
            {
                if (hit.normal.normalized == new Vector3(0, 1, 0))
                {
                    ChangeState(State.AVOID2);
                    break;
                }
                else
                {
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
            if (Physics.Raycast(rays[i], out hit, 3))
            {
                hasHitted = true;
                break;
            }
            Debug.DrawRay(rays[i].origin, rays[i].direction * 3, Color.red);
        }

        if (!hasHitted)
        {
            ChangeState(State.CHASE);
        }
    }
}
