using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Graph", menuName = "ScriptableObjects/Graphs", order = 1)]
public class GraphObject /*: ScriptableObject*/
{
    [HideInInspector]
    public int ID;
    [HideInInspector]
    public List<Node> Graph = new List<Node>();
}
