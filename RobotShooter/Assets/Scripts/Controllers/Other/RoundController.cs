using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class RoundController : MonoBehaviour
{
    public enum State { INITIAL, PREPARATION, SPAWN, FIGHT, CLEAR};
    public State currentState = State.INITIAL;

    [Header("EnemySpawns")]
    public int maxEnemiesOnScreen;
    public int minEnemiesForNextPeak; //If enemies are below this number, next peak will start
    public int firstRoundTotalEnemies;
    public Enemy[] enemies;
    public Transform[] groundedEnemySpawners;
    public Transform[] airEnemySpawners;
    public int[] peakEnemySpawnPercentages; //Percantage of each kind of enemy depending on the peak (the leagth of the array is the number of peaks for each round)

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

    // Start is called before the first frame update
    void Start()
    {
        currentMap = 2; //Remove in the future!!!
        currentRound = 0;
        currentPeak = 0;
        elapsedTime = 0;
        roundTotalEnemies = firstRoundTotalEnemies;
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
                if (elapsedTime >= spawnTime) ChangeState(State.FIGHT);
                break;
            case State.FIGHT:
                if (elapsedTime >= fightTime) //O enemics més petit que el mínim
                {
                    if (currentPeak < peakEnemySpawnPercentages.Length) ChangeState(State.SPAWN);
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
                SpawnEnemies();
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
        Map currentMap = null;
        foreach (Map m in maps)
        {
            if (m.id == this.currentMap) currentMap = m;
        }

        int peakTotalEnemies = roundTotalEnemies * peakEnemySpawnPercentages[currentPeak - 1] / 100; //This number represents the amout of enemies that will be spawned during this peak.
        int[] peakTotalEnemiesPerEnemyType = new int[enemies.Length]; //This array represents the amout of enemies of each type that will be spawned during this peak.
        float[] spawnRateTimePerEnemyType = new float[enemies.Length]; //This array represents the time between the enemy spawns of each type during this peak.

        for (int i = 0; i < enemies.Length; i++)
        {
            peakTotalEnemiesPerEnemyType[i] = peakTotalEnemies * currentMap.enemySpawnPercentage[i] / 100;
            spawnRateTimePerEnemyType[i] = spawnTime / peakTotalEnemiesPerEnemyType[i];
            StartCoroutine(SpawnTimer(i, spawnRateTimePerEnemyType[i]));
        }
    }

    IEnumerator SpawnTimer(int id, float time)
    {
        Transform spawnPoint;
        if (enemies[id].dimension == Enemy.Dimension.GROUND) spawnPoint = groundedEnemySpawners[UnityEngine.Random.Range(0, groundedEnemySpawners.Length)];
        else spawnPoint = airEnemySpawners[UnityEngine.Random.Range(0, airEnemySpawners.Length)];
        Instantiate(enemies[id].enemyPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

        yield return new WaitForSeconds(time);
        if (currentState == State.SPAWN) StartCoroutine(SpawnTimer(id, time));
    }

    void LoadCurrentMap()
    {
        foreach (Map m in maps)
        {
            m.mapPrefab.SetActive(false);
            //m.bake.SetActive(false);
            if (m.id == currentMap)
            {
                m.mapPrefab.SetActive(true);
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
