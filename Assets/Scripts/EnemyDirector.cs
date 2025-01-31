using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float pointsPerSecond = 1f;
    [SerializeField] private float minSpawnDistance = 10f;
    [SerializeField] private float maxSpawnDistance = 20f;
    [SerializeField] private int maxEnemiesAtATime = 15;
    [SerializeField] private int smallestWave = 5;
    [SerializeField] private float enemyDespawnDistance = 30f;

    private List<GameObject> enemiesSpawned = new List<GameObject>();
    private Transform player;
    private float points = 0f;
    private float waveSpawnTries = 5;
    private float intensity = 1;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        determineIntensity();
        points += pointsPerSecond * Time.deltaTime * intensity;
    }

    void determineIntensity()
    {
        if (enemiesSpawned.Count >= maxEnemiesAtATime)
        {
            intensity = 0.15f;
        }

        intensity = 1f;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawnEnemies();
            CleanDistantEnemies();
        }
    }

    void TrySpawnEnemies()
    {
        enemiesSpawned.RemoveAll(e => e == null);
        if (enemiesSpawned.Count >= maxEnemiesAtATime) return;

        List<EnemyData> affordableEnemies = enemyDatabase.enemyList.FindAll(e => e.cost <= points).OrderByDescending(e => e.cost).ToList();
        if (affordableEnemies.Count == 0) return;

        List<EnemyData> chosenEnemies = new List<EnemyData>();
        int tries = 0;
        while (tries < waveSpawnTries && chosenEnemies.Count < smallestWave)
        {
            chosenEnemies = GetWave(affordableEnemies);
            tries++;
        }
        if (chosenEnemies.Count < smallestWave) chosenEnemies.Clear();

        Vector3 spawnPos = GetSpawnPosition();

        if (spawnPos != Vector3.zero)
        {
            foreach (EnemyData enemy in chosenEnemies)
            {
                Vector3 spawnVariance = (new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0f)).normalized;

                var e = Instantiate(enemy.enemyPrefab, spawnPos + spawnVariance, Quaternion.identity);
                enemiesSpawned.Add(e);
                points -= enemy.cost;
            }
        }
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 randomPos = Random.insideUnitSphere;
        randomPos.z = 0;
        randomPos.Normalize();
        randomPos = randomPos * minSpawnDistance + (Random.Range(0, maxSpawnDistance - minSpawnDistance) * randomPos);

        return randomPos + player.transform.position;
    }

    List<EnemyData> GetWave(List<EnemyData> affordableEnemies)
    {
        float tempPoints = points;
        List<EnemyData> enemiesToBuy = new List<EnemyData>();
        while (enemiesToBuy.Count < maxEnemiesAtATime - enemiesSpawned.Count && affordableEnemies.Count > 0)
        {
            int randomEnemyToBuy = Mathf.Max(Random.Range(0, affordableEnemies.Count), Random.Range(0, affordableEnemies.Count));
            EnemyData randomEnemy = affordableEnemies[randomEnemyToBuy];
            tempPoints -= randomEnemy.cost;
            enemiesToBuy.Add(randomEnemy);

            affordableEnemies.RemoveAll(e => e.cost >= tempPoints);
        }

        return enemiesToBuy;
    }

    void CleanDistantEnemies()
    {
        foreach (GameObject e in enemiesSpawned)
        {
            if ((e.transform.position - player.transform.position).magnitude > enemyDespawnDistance) Destroy(e);
        }
        enemiesSpawned.RemoveAll(e => (e.transform.position - player.transform.position).magnitude > enemyDespawnDistance);
    }
}
