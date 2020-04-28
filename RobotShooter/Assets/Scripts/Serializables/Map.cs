using System;
using UnityEngine;

[Serializable]
public class Map
{
    public int id;
    public enum Difficulty { EASY, MEDIUM, HARD };
    public Difficulty difficulty;
    public int[] enemySpawnPercentage;
    public GameObject mapPrefab; //Remove in the future!!!
    public GameObject bake;
}
