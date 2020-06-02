using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Connection", menuName = "ScriptableObjects/Connection", order = 1)]
public class Connection : ScriptableObject
{
    public Node successor;
    public int cost;
}
