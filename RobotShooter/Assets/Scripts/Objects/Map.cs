using System;
using UnityEngine;

[Serializable]
public class Map
{
    private const int ENEMIES = 2;

    public int id;
    public enum Difficulty { EASY, MEDIUM, HARD };
    public Difficulty difficulty;
    public int[] enemySpawnPercentage = new int[ENEMIES];
    public GameObject map; //Remove in the future!!!
    public GameObject bake;

    void OnValidate()
    {
        if (enemySpawnPercentage.Length != ENEMIES)
        {
            Debug.LogWarning("Enemy Spawn Percentage array length cannot be changed on the inspector, please ask MCreeper00 if you want more info :)");
            Array.Resize(ref enemySpawnPercentage, ENEMIES);
        }
    }
}
