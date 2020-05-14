using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder3D
{
    public Vector3 goal { get; set; }
    Node startNode;
    Node endNode;
    Node current;
    List<Node> openList;
    List<Node> closedList;
    public bool pathFound = false;

    //PathFollowing
    public float wayPointReachedRadius = 0.2f;
    public int currentWayPointIndex = 0;

    public Pathfinder3D()
    {
        openList = new List<Node>();
        closedList = new List<Node>();
    }

    public Path3D AStar(GameObject agent)
    {
        openList.Clear();
        closedList.Clear();

        startNode = ProvisionalManager.Instance.currentGraph.Graph[0];
        endNode = ProvisionalManager.Instance.currentGraph.Graph[0];
        foreach (Node n in ProvisionalManager.Instance.currentGraph.Graph)
        {
            if (DistanceToTarget(n.position, agent.transform.position) < DistanceToTarget(startNode.position, agent.transform.position))
            {
                startNode = n;
            }

            if (DistanceToTarget(n.position, goal) < DistanceToTarget(endNode.position, goal))
            {
                endNode = n;
            }
        }
        
        //Registre startNode
        startNode.predecessor = null;
        startNode.costFromStart = 0;
        startNode.estimatedCostToGoal = Heuristic(startNode, endNode);

        openList.Add(startNode);

        current = startNode;
        
        while (openList.Count > 0 && !pathFound)
        {
            foreach (Node n in openList)
            {
                if (n.estimatedCostToGoal < current.estimatedCostToGoal)
                {
                    current = n;
                }
            }
            
            if (current == endNode)
            {
                pathFound = true;
            }
            else
            {
                foreach (Connection connection in current.Connections)
                {
                    Node successor = connection.successor;
                    
                    if (ContainsLoop(openList, successor) == false && ContainsLoop(closedList, successor) == false)
                    {
                        successor.costFromStart = current.costFromStart + connection.cost;
                        successor.estimatedCostToGoal = current.costFromStart + connection.cost + Heuristic(successor, endNode);
                        openList.Add(successor);
                    }/*
                    else if (openList.Contains(successor))
                    {
                        if (current.costFromStart + connection.cost < successor.costFromStart)
                        {
                            successor.predecessor = current;
                            successor.costFromStart = current.costFromStart + connection.cost;
                            successor.estimatedCostToGoal = current.costFromStart + connection.cost + Heuristic(successor, endNode);
                            connection.successor = successor;
                        }
                    }
                    else if (closedList.Contains(successor))
                    {
                        if (current.costFromStart + connection.cost < successor.costFromStart)
                        {
                            successor.predecessor = current;
                            successor.costFromStart = current.costFromStart + connection.cost;
                            successor.estimatedCostToGoal = current.costFromStart + connection.cost + Heuristic(successor, endNode);
                            connection.successor = successor;

                            closedList.Remove(successor);
                            openList.Add(successor);
                        }
                    }*/
                }
            }

            openList.Remove(current);
            closedList.Add(current);            
        }

        if (pathFound)
        {
            Path3D p = new Path3D();
            Node c = endNode;
            p.Path.Add(c);
            for (int i = 1; c != startNode; i++)
            {
                p.Path.Add(c.predecessor);
                c = c.predecessor;
            }

            p.Path.Reverse();

            Node finalNode = new Node();
            finalNode.position = goal;
            p.Path.Add(finalNode);

            return p;
        }
        else
        {
            return null;
        }
    }

    int Heuristic(Node start, Node end)
    {
        //return Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(start.position.x - end.position.x, 2) + Mathf.Pow(start.position.y - end.position.y, 2) + Mathf.Pow(start.position.z - end.position.z, 2)));
        //return 0;
        float dx = Mathf.Abs(start.position.x - end.position.x);
        float dy = Mathf.Abs(start.position.y - end.position.y);
        float dz = Mathf.Abs(start.position.z - end.position.z);
        return Mathf.RoundToInt((dx + dy + dz) - (2 - Mathf.Sqrt(2)) * Mathf.Min(Mathf.Min(dx, dy), dz));
    }

    float DistanceToTarget(Vector3 me, Vector3 target)
    {
        return (target - me).magnitude;
    }

    float DistanceToTargetSquared(Vector3 me, Vector3 target)
    {
        return (target - me).sqrMagnitude;
    }

    bool ContainsLoop(List<Node> list, Node value)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == value)
            {
                return true;
            }
        }
        return false;
    }
}
