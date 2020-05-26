using UnityEngine;

public class ObjectPoolerManager : AController
{

    [Header("AR Bullet")]
    public GameObject ARBullet;
    public Transform ARBulletParent;
    public int ARBulletAmount;
    [HideInInspector] public ObjectPooler ARBulletOP;

    [Header("Air Enemy Bullet")]
    public GameObject airEnemyBullet;
    public Transform airEnemyBulletParent;
    public int airEnemyBulletAmount;
    [HideInInspector] public ObjectPooler airEnemyBulletOP;

    [Header("Tank Enemy Bullet")]
    public GameObject tankEnemyBullet;
    public Transform tankEnemyBulletParent;
    public int tankEnemyBulletAmount;
    [HideInInspector] public ObjectPooler tankEnemyBulletOP;

    [HideInInspector] public ObjectPooler[] enemiesOP;

    public void StartGame()
    {
        ARBulletOP = new ObjectPooler(ARBulletAmount, ARBullet, ARBulletParent);
        airEnemyBulletOP = new ObjectPooler(airEnemyBulletAmount, airEnemyBullet, airEnemyBulletParent);
        tankEnemyBulletOP = new ObjectPooler(tankEnemyBulletAmount, tankEnemyBullet, tankEnemyBulletParent);

        Enemy[] enemies = gc.roundController.enemies;
        enemiesOP = new ObjectPooler[enemies.Length];
        for (int i = 0; i < enemiesOP.Length; i++)
        {
            enemiesOP[i] = new ObjectPooler(enemies[i].enemyMaxAmount, enemies[i].enemyPrefab, enemies[i].enemyParent);
        }
    }

}