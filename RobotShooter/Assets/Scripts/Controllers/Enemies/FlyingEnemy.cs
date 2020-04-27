using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    public enum State { INITIAL, CHASE, ATTACK, GO_BACK, HIT, DEATH }
    public State currentState = State.INITIAL;

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

    // Start is called before the first frame update
    void Start()
    {
        
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
                transform.LookAt(direction);
                AvoidObstacles();
                if (elapsedTime >= 0.5f)
                {
                    direction = (player.transform.position - transform.position).normalized;
                    elapsedTime = 0;
                }
                transform.position += direction * speed * Time.deltaTime;
                elapsedTime += Time.deltaTime;
                break;
            case State.ATTACK:
                if (health <= 0)
                {
                    ChangeState(State.DEATH);
                    break;
                }
                break;
            case State.GO_BACK:
                if (health <= 0)
                {
                    ChangeState(State.DEATH);
                    break;
                }
                break;
            case State.HIT:
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
                direction = (player.transform.position - transform.position).normalized;
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
        Vector3 rayDirection;
        int lastRay;
        RaycastHit hit;
        /*
        for (int r = -1; r < 3; r++)
        {
            for (int c = -1; c < 3; c++)
            {
                //rayDirection = new Vector3(transform.forward.x + (Mathf.Cos(c) * Mathf.Cos(r)), transform.forward.y + (Mathf.Cos(c) * Mathf.Sin(r)), Mathf.Sin(c));
                //rayDirection = new Vector3((transform.forward + (c * transform.right)).normalized, transform.forward + (c * transform.right).normalized), 0);
                //rayDirection = Quaternion.Euler(0, rayAngle, 0).eulerAngles * transform.forward;
                if (Physics.Raycast(transform.position, rayDirection, out hit, Mathf.Infinity))
                {
                    
                }
                Debug.DrawRay(transform.position, rayDirection * 1000, Color.red);
            }
        }*/
        
        for (int i = 0; i < 9; i++)
        {
            rayDirection = (rayPoints[i].transform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, rayDirection, out hit, 2))
            {
                direction = new Vector3(transform.forward.x + hit.normal.x, 0, transform.forward.z + hit.normal.z);
            }
            Debug.DrawRay(transform.position, rayDirection * 2, Color.red);
        }
    }
}
