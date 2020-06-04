using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "Connection", menuName = "ScriptableObjects/Connection", order = 1)]
//[Serializable]
public class Connection /*: ScriptableObject*/
{
    public Node successor;
    public int cost;
}
