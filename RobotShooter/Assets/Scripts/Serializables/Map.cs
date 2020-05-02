using System;
using UnityEngine;

[Serializable]
public class Map
{
    public int id;
    public enum Difficulty { EASY, MEDIUM, HARD };
    public Difficulty difficulty;
    [Tooltip("Each slot represents the spawn percentage of each type of enemy on this map. The order of the enemies is the same as the Enemy list.")]
    public int[] enemySpawnPercentage;
    public GameObject map; //Remove in the future!!!
    [Tooltip("Bake of this map navmesh.")]
    public GameObject bake;
}
