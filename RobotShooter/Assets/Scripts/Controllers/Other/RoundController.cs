using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class RoundController : AController
{
    public enum State { INITIAL, PREPARATION, SPAWN, FIGHT, CLEAR};
    public State currentState = State.INITIAL;

    [Header("EnemySpawns")]
    [Tooltip("Maximum number of enemies that will be displayed on screen.")]
    public int maxEnemiesOnScreen;
    [Tooltip("Total of enemies that will spawn during the first round, that total will be increased every round.")]
    public int firstRoundTotalEnemies;
    [Tooltip("Every round, the total number of enemies that will spawn during that round is increased by this number.")]
    public int totalEnemiesIncrementPerRound;
    [Tooltip("If the box is checked, the minimum amount of enemies on the arena to trigger the next peak will be calculated through a percentage of the total enemies of the peak, elsewise, it will allways be a flat number.")]
    public bool useMinEnemiesPercentage; //If this is true, minEnemies will be calculated through a percentage instead of a flat number
    [Tooltip("Only use this field if Use Min Enemies Percentage box is checked")]
    public int minEnemiesPercentage; //This percentage allow you to canculate minEnemies depending on the enemies of each peak
    [Tooltip("Only use this field if Use Min Enemies Percentage box is unchecked")]
    public int minEnemies; //If enemies are below this number, next peak will start
    [Tooltip("Note: The losses on enemy spawn calculation will be added on the last enemy on this list, this means that the enemy which spawns more often should go last.")]
    public Enemy[] enemies;
    public Transform[] groundedEnemySpawners;
    public Transform[] airEnemySpawners;
    [Tooltip("The length of this list is the number of peaks each round have. Each slot on the list will represent a peak, fill them with the percentage of total round enemies that should spawn during each peak.")]
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
    public int enemiesSpawnedOnCurrentRound; //This number keep track of the number of enemies that are spawned on each peak;
    public int[] extraEnemies; //Number of enemies that could not be spawned due to totalEnemies > maxEnemies

    private float elapsedTime;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    if (useMinEnemiesPercentage) minEnemies = 0;

    //    currentRound = 0;
    //    currentPeak = 0;
    //    currentMap = 2; //Remove in the future!!!

    //    roundTotalEnemies = firstRoundTotalEnemies - totalEnemiesIncrementPerRound;
    //    currentEnemies = 0;
    //    elapsedTime = 0;
    //    enemiesSpawnedOnCurrentRound = 0;
    //    extraEnemies = new int[enemies.Length];
    //}

    public void StartGame()
    {
        if (useMinEnemiesPercentage) minEnemies = 0;

        currentRound = 0;
        currentPeak = 0;
        currentMap = 2; //Remove in the future!!!        

        roundTotalEnemies = firstRoundTotalEnemies - totalEnemiesIncrementPerRound;
        currentEnemies = 0;
        elapsedTime = 0;
        enemiesSpawnedOnCurrentRound = 0;
        extraEnemies = new int[enemies.Length];

        ChangeState(State.PREPARATION);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.INITIAL:
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
                if (gc.uiController != null) gc.uiController.IncreaseRound();
                roundTotalEnemies += totalEnemiesIncrementPerRound;
                enemiesSpawnedOnCurrentRound = 0;
                if (currentRound > 1)
                {
                    GameEvents.instance.RoundChange();
                }
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
        if (currentPeak == peakEnemySpawnPercentages.Length) peakTotalEnemies = roundTotalEnemies - enemiesSpawnedOnCurrentRound; //The remaining enemies are spawned on the last peak (this avoid losses on the integer division)
        enemiesSpawnedOnCurrentRound += peakTotalEnemies;
        int[] peakTotalEnemiesPerEnemyType = new int[enemies.Length]; //This array represents the amout of enemies of each type that will be spawned during this peak.
        float[] spawnRateTimePerEnemyType = new float[enemies.Length]; //This array represents the time between the enemy spawns of each type during this peak.
        int enemiesSpawnedOnCurrentPeak = 0;

        if (useMinEnemiesPercentage) minEnemies = (peakTotalEnemies + currentEnemies) * minEnemiesPercentage / 100;

        for (int i = 0; i < enemies.Length; i++)
        {
            peakTotalEnemiesPerEnemyType[i] = peakTotalEnemies * currentMap.enemySpawnPercentage[i] / 100;
            if (i == enemies.Length - 1) peakTotalEnemiesPerEnemyType[i] = peakTotalEnemies - enemiesSpawnedOnCurrentPeak; //The remaining enemies are from the last enemy type/id (this avoid losses on the integer division)
            enemiesSpawnedOnCurrentPeak += peakTotalEnemiesPerEnemyType[i];
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

    void Debuging()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Backspace)) DecreaseEnemyCount();
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Return)) currentEnemies++;
#endif
    }

}
