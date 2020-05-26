using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolableObject
{
    public string name;
    public GameObject prefab;
    public Transform parent;
    public int amount;
    [HideInInspector] public ObjectPooler objectPooler;
}
