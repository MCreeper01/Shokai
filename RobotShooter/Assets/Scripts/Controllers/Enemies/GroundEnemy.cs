using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : MonoBehaviour
{
    NavMeshAgent agent;
    public Transform point;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = point.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
