using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 position;
    public Node predecessor;
    public int costFromStart;
    public int estimatedCostToGoal;
    public List<Connection> Connections = new List<Connection>();
}
