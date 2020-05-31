using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Node", menuName = "ScriptableObjects/Node", order = 1)]
public class Node : ScriptableObject
{
    public Vector3 position;
    public Node predecessor;
    public int costFromStart;
    public int estimatedCostToGoal;
    public List<Connection> Connections = new List<Connection>();
}
