using System;
using UnityEngine;

[Serializable]
public class Enemy
{
    public string name;
    public GameObject enemyPrefab;
    public Transform enemyParent;
    public int enemyMaxAmount;
    public enum Dimension { GROUND, AIR };
    public Dimension dimension;
}
