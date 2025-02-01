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
    [SerializeField] private float maxStoredPoints = 30f;
    [SerializeField] private float minSpawnDistance = 10f;
    [SerializeField] private float maxSpawnDistance = 20f;
    [SerializeField] private int maxEnemiesAtATime = 15;
    [SerializeField] private int smallestWave = 5;
    [SerializeField] private float enemyDespawnDistance = 30f;
    [SerializeField] private bool spawnAtHuts = true;

    private ChunkManager levelGen;
    private List<GameObject> enemiesSpawned = new List<GameObject>();
    private Transform player;
    private float points = 0f;
    private float waveSpawnTries = 5;
    private float intensity = 1;

    void Start()
    {
        levelGen = ChunkManager.Instance;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        CheckHutInView();
        determineIntensity();
        points += pointsPerSecond * Time.deltaTime * intensity;

        if (points > maxStoredPoints) points = maxStoredPoints;
    }

    void determineIntensity()
    {
        if (enemiesSpawned.Count >= maxEnemiesAtATime)
        {
            intensity = 0.3f;
            return;
        }

        if (spawnAtHuts)
        {
            intensity = 2f;
            return;
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

        List<GameObject> closestHuts = new List<GameObject>();
        Vector2Int closeHutPos = Vector2Int.zero;
        foreach (var (outpost, pos) in levelGen.ClosestOutposts)
        {
            if (pos != null && levelGen.OutpostsInChunk.ContainsKey((Vector2Int)pos))
            {
                levelGen.OutpostsInChunk[(Vector2Int)pos].RemoveAll(e => e == null);

                if (closestHuts.Count == 0) {
                    closestHuts = levelGen.OutpostsInChunk[(Vector2Int)pos];
                    closeHutPos = (Vector2Int)pos;
                    continue;
                }

                float outpostDistance = ((new Vector3(pos.Value.x, pos.Value.y, 0) * (float)ChunkManager.CHUNK_SIZE) - player.transform.position).magnitude;
                float prevOutpostDistance = ((new Vector3(closeHutPos.x, closeHutPos.y, 0) * (float)ChunkManager.CHUNK_SIZE) - player.transform.position).magnitude;

                if (outpostDistance < prevOutpostDistance)
                {
                    closestHuts = levelGen.OutpostsInChunk[(Vector2Int)pos];
                    closeHutPos = (Vector2Int)pos;
                }
            }
        }

        if (closestHuts.Count > 0)
        {
            closestHuts.Shuffle();
            foreach (var hut in closestHuts)
            {
                if (IsTransformInView(Camera.main, hut.transform)) return hut.transform.position - new Vector3(0.0f, 1.6f, 0.0f);
            }
        }

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

    private void CheckHutInView()
    {
        foreach (var (outpost, pos) in levelGen.ClosestOutposts)
        {
            if (pos != null && levelGen.OutpostsInChunk.ContainsKey((Vector2Int)pos))
            {
                foreach (var hut in levelGen.OutpostsInChunk[(Vector2Int)pos])
                {
                    if (IsTransformInView(Camera.main, hut.transform))
                    {
                        spawnAtHuts = true;
                        return;
                    }
                }
            }
        }
        spawnAtHuts = false;
    }

    private bool IsTransformInView(Camera cam, Transform target)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);

        return viewportPos.x >= 0 && viewportPos.x <= 1 &&
               viewportPos.y >= 0 && viewportPos.y <= 1;
    }
}
