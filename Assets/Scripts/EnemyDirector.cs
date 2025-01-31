using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    [Header("Director Spawn Settings")]
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public int cost;
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Enemy Database")]
public class EnemyDatabase : ScriptableObject
{
    public List<EnemyData> enemyList = new List<EnemyData>();
}

public class EnemyDirector : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyDatabase enemyDatabase;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float pointsPerSecond = 1f;
    [SerializeField] private float minSpawnDistance = 10f;
    [SerializeField] private float maxSpawnDistance = 20f;
    [SerializeField] private int maxEnemiesAtATime = 15;

    private List<GameObject> enemiesSpawned = new List<GameObject>();
    private Transform player;
    private float points = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        points += pointsPerSecond * Time.deltaTime;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawnEnemies();
        }
    }

    void TrySpawnEnemies()
    {
        enemiesSpawned.RemoveAll(e => e == null);
        if (enemiesSpawned.Count >= maxEnemiesAtATime) return;

        List<EnemyData> affordableEnemies = enemyDatabase.enemyList.FindAll(e => e.cost <= points);
        if (affordableEnemies.Count == 0) return;

        EnemyData chosenEnemy = affordableEnemies[Random.Range(0, affordableEnemies.Count)];

        Vector3 spawnPos = GetSpawnPosition();
        if (spawnPos != Vector3.zero)
        {
            var e = Instantiate(chosenEnemy.enemyPrefab, spawnPos, Quaternion.identity);
            enemiesSpawned.Add(e);
            points -= chosenEnemy.cost;
        }
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 randomPos = Random.insideUnitSphere;
        randomPos.z = 0;
        randomPos.Normalize();
        randomPos = randomPos * minSpawnDistance + (Random.Range(0, maxSpawnDistance - minSpawnDistance) * randomPos);

        return randomPos;
    }
}
