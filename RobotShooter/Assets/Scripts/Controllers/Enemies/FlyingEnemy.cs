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
}
