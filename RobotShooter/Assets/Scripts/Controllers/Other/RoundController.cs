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
    public int firstRoundTotalEnemies;
    public int totalEnemiesIncrementPerRound;
    public int minEnemiesPercentage;
    public Enemy[] enemies;
    public Transform[] groundedEnemySpawners;
    public Transform[] airEnemySpawners;
    public int[] peakEnemySpawnPercentages; //Percantage of each kind of enemy depending on the peak (the leagth of the array is the number of peaks for each round)

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

    public int roundTotalEnemies;
    public int currentEnemies; //Total enemies on screen
    public int minEnemies; //If enemies are below this number, next peak will start
    public int[] extraEnemies; //Number of enemies that could not be spawned due to totalEnemies > maxEnemies

    private float elapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        currentRound = 0;
        currentPeak = 0;
        currentMap = 2; //Remove in the future!!!

        roundTotalEnemies = firstRoundTotalEnemies - totalEnemiesIncrementPerRound;
        currentEnemies = 0;
        minEnemies = 0;
        elapsedTime = 0;
        extraEnemies = new int[enemies.Length];
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
                //If it's not the last peak and the spawning time it's over or you killed enought enemies, start the next peak.
                if (currentPeak < peakEnemySpawnPercentages.Length && (elapsedTime >= fightTime || currentEnemies <= minEnemies)) ChangeState(State.SPAWN);
                if (currentEnemies <= 0) ChangeState(State.CLEAR);
                break;
            case State.CLEAR:
                if (currentEnemies <= 0) ChangeState(State.PREPARATION);
                break;
        }

        elapsedTime += Time.deltaTime;
        Debuging();
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
                roundTotalEnemies += totalEnemiesIncrementPerRound;
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

        minEnemies = (peakTotalEnemies + currentEnemies) * minEnemiesPercentage / 100;

        for (int i = 0; i < enemies.Length; i++)
        {
            peakTotalEnemiesPerEnemyType[i] = peakTotalEnemies * currentMap.enemySpawnPercentage[i] / 100;
            spawnRateTimePerEnemyType[i] = spawnTime / peakTotalEnemiesPerEnemyType[i];
            StartCoroutine(SpawnTimer(i, spawnRateTimePerEnemyType[i]));
        }

    }

    IEnumerator SpawnTimer(int id, float time)
    {
        if (currentEnemies <= maxEnemiesOnScreen) InstantiateEnemy(id);
        else extraEnemies[id]++;

        yield return new WaitForSeconds(time);
        if (currentState == State.SPAWN) StartCoroutine(SpawnTimer(id, time));
    }

    void InstantiateEnemy(int id)
    {
        Transform spawnPoint;
        if (enemies[id].dimension == Enemy.Dimension.GROUND) spawnPoint = groundedEnemySpawners[UnityEngine.Random.Range(0, groundedEnemySpawners.Length)];
        else spawnPoint = airEnemySpawners[UnityEngine.Random.Range(0, airEnemySpawners.Length)];
        Instantiate(enemies[id].enemyPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        currentEnemies++;
    }

    public void DecreaseEnemyCount()
    {
        currentEnemies--;

        for (int i = 0; i < extraEnemies.Length; i++)
        {
            if (extraEnemies[i] > 0)
            {
                InstantiateEnemy(i);
                extraEnemies[i]--;
                break;
            }
        }
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

    void Debuging()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Backspace)) DecreaseEnemyCount();
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Return)) currentEnemies++;
#endif
    }

}
