using System;
using UnityEngine;

[Serializable]
public class Map
{
    public int id;
    public enum Difficulty { EASY, MEDIUM, HARD };
    public Difficulty difficulty;
    public float[] enemySpawnPercentage;
    public GameObject map; //Remove in the future!!!
}
