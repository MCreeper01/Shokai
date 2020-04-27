using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundController : MonoBehaviour
{
    public enum State { INITIAL, PREPARATION, SPAWN, FIGHT, CLEAR};
    public State currentState = State.INITIAL;

    [Header("EnemySpawns")]
    public int maxEnemiesOnScreen;
    private int minEnemies; //If enemies are below this number, next peak will start
    public int[] peakEnemySpawnPercentages = new int[PEAKS];
    public GameObject[] enemySpawners;

    private int roundTotalEnemies;
    private int currentEnemies; //Total enemies on screen
    private int extraEnemies; //Number of enemies that could not be spawned due to totalEnemies > maxEnemies

    [Header("Timers")]
    public float preparationTime;
    public float spawnTime;
    public float fightTime;

    [Header("Maps")]
    public Map[] maps;

    [Header("Debug")]
    public int currentPeak;
    public int currentRound;
    public int currentMap;

    private float elapsedTime;

    private const int PEAKS = 3;

    void OnValidate()
    {
        if (peakEnemySpawnPercentages.Length != PEAKS)
        {
            Debug.LogWarning("Peak Percentage array length cannot be changed on the inspector, please ask MCreeper00 if you want more info :)");
            Array.Resize(ref peakEnemySpawnPercentages, PEAKS);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentMap = 2; //Remove in the future!!!
        currentRound = 0;
        currentPeak = 0;
        elapsedTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.INITIAL:
                ChangeState(State.PREPARATION);
                break;
            case State.PREPARATION:
                if (elapsedTime >= preparationTime) ChangeState(State.SPAWN);
                break;
            case State.SPAWN:
                SpawnEnemies();
                if (elapsedTime >= spawnTime) ChangeState(State.FIGHT);
                break;
            case State.FIGHT:
                if (elapsedTime >= fightTime) //O enemics més petit que el mínim
                {
                    if (currentPeak < PEAKS) ChangeState(State.SPAWN);
                    else ChangeState(State.CLEAR);
                }
                break;
            case State.CLEAR:
                if (currentEnemies <= 0) ChangeState(State.PREPARATION);
                break;
        }

        elapsedTime += Time.deltaTime;
    }

    void ChangeState(State newState)
    {
        // exit logic
        switch (currentState)
        {
            case State.INITIAL:
                break;
            case State.PREPARATION:
                break;
            case State.SPAWN:
                break;
            case State.FIGHT:
                break;
            case State.CLEAR:
                break;
        }

        // enter logic
        switch (newState)
        {
            case State.INITIAL:
                break;
            case State.PREPARATION:
                currentPeak = 0;
                currentRound++;
                if (currentMap == 1) currentMap = 2; //Remove in the future!!!
                else currentMap = 1; //Remove in the future!!!
                LoadCurrentMap();
                break;
            case State.SPAWN:
                currentPeak++;
                break;
            case State.FIGHT:
                break;
            case State.CLEAR:
                break;
        }

        elapsedTime = 0;
        currentState = newState;
    }

    void SpawnEnemies()
    {
        roundTotalEnemies = 30; //remove later

        Map currentMap = null;
        foreach (Map m in maps)
        {
            if (m.id == this.currentMap) currentMap = m;
        }

        int peakTotalEnemies = roundTotalEnemies * peakEnemySpawnPercentages[currentPeak - 1] / 100; //This number represents the amout of enemies that will be spawned during this peak.
        int[] peakTotalEnemiesPerEnemyType = new int[currentMap.enemySpawnPercentage.Length]; //This array represents the amout of enemies of each type that will be spawned during this peak.
        float[] spawnRateTimePerEnemyType = new float[peakTotalEnemiesPerEnemyType.Length]; //This array represents the time between the enemy spawns of each type during this peak.

        for (int i = 0; i < peakTotalEnemiesPerEnemyType.Length; i++)
        {
            peakTotalEnemiesPerEnemyType[i] = peakTotalEnemies * currentMap.enemySpawnPercentage[i] / 100;
            spawnRateTimePerEnemyType[i] = spawnTime / peakTotalEnemiesPerEnemyType[i];
        }
    }

    void LoadCurrentMap()
    {
        foreach (Map m in maps)
        {
            m.map.SetActive(false);
            //m.bake.SetActive(false);
            if (m.id == currentMap)
            {
                m.map.SetActive(true);
                //m.bake.SetActive(true);
            }
        }

        switch (currentMap)
        {
            case 1:
                break;
            case 2:
                break;
        }
    }

}
