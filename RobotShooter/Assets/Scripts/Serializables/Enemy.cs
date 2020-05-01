using System;
using UnityEngine;

[Serializable]
public class Enemy
{
    public string name;
    public GameObject enemyPrefab;
    public enum Dimension { GROUND, AIR };
    public Dimension dimension;
}
