using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public int ExtraEnemyHealthTime { get; private set; }
    private bool extraHealthIncreasing = false;

    public static EnemyDirector Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        levelGen = ChunkManager.Instance;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnLoop());
        ExtraEnemyHealthTime = 0;
    }

    void Update()
    {
        CheckHutInView();
        determineIntensity();
        points += pointsPerSecond * Time.deltaTime * intensity;
        maxEnemiesAtATime = PowerGameState.Instance.PowString.Length + 3;
        maxStoredPoints = PowerGameState.Instance.PowString.Length * 10;

        if (PowerGameState.Instance.PowString.Length >= 6 && !extraHealthIncreasing)
        {
            StartCoroutine(extraHealth());
        }

        if (points > maxStoredPoints) points = maxStoredPoints;
    }

    void determineIntensity()
    {
        intensity = PowerGameState.Instance.PowString.Length * .33f;

        if (enemiesSpawned.Count >= maxEnemiesAtATime)
        {
            intensity *= 0.33f;
            return;
        }

        if (spawnAtHuts)
        {
            intensity *= 2f;
            return;
        }
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

        foreach (EnemyData enemy in chosenEnemies)
        {
            // This is legit a fucking crime
            // If I knew how this was going to be originaly implimented I wouldnt
            // have made this such a fucking mess, I am sorry :(
            (Vector3 spawnPos, OutpostType outpostType) = GetSpawnPosition();
            var enemyReal = enemyDatabase.enemyList.Find(e => e.type == outpostType);

            if (spawnPos != Vector3.zero)
            {
                Vector3 spawnVariance = (new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0f)).normalized;

                var e = Instantiate(enemyReal.enemyPrefab, spawnPos + spawnVariance, Quaternion.identity);
                if (e.TryGetComponent<EnemyHealth>(out EnemyHealth eh))
                {
                    eh.Health += ExtraEnemyHealthTime;
                }
                enemiesSpawned.Add(e);
                points -= enemy.cost;
            }
        }
    }

    private (Vector3, OutpostType) GetSpawnPosition()
    {

        List<GameObject> closestHuts = new List<GameObject>();
        Vector2Int closeHutPos = Vector2Int.zero;
        System.Random rng = new System.Random();

        foreach (var (outpost, pos) in levelGen.ClosestOutposts.OrderBy(x => rng.Next()))
        {
            if (pos != null && levelGen.OutpostsInChunk.ContainsKey((Vector2Int)pos))
            {
                levelGen.OutpostsInChunk[(Vector2Int)pos].RemoveAll(e => e == null);

                foreach (var hut in levelGen.OutpostsInChunk[(Vector2Int)pos].OrderBy(x => rng.Next()))
                {
                    if (IsTransformInView(Camera.main, hut.transform)) return (hut.transform.position - new Vector3(0.0f, 1.6f, 0.0f), outpost);
                }

                /*float outpostDistance = ((new Vector3(pos.Value.x, pos.Value.y, 0) * (float)ChunkManager.CHUNK_SIZE) - player.transform.position).magnitude;
                float prevOutpostDistance = ((new Vector3(closeHutPos.x, closeHutPos.y, 0) * (float)ChunkManager.CHUNK_SIZE) - player.transform.position).magnitude;

                if (outpostDistance < prevOutpostDistance)
                {
                    outpostData = outpost;
                    closestHuts = levelGen.OutpostsInChunk[(Vector2Int)pos];
                    closeHutPos = (Vector2Int)pos;
                }*/
            }
        }


        /*if (closestHuts.Count > 0)
        {
            closestHuts.Shuffle();
            foreach (var hut in closestHuts)
            {
                if (IsTransformInView(Camera.main, hut.transform)) return (hut.transform.position - new Vector3(0.0f, 1.6f, 0.0f), OutPost);
            }
        }*/

        return (Vector3.zero, OutpostType.P);

        // Enemies can only spawn at huts now

        /*
        Vector3 randomPos = Random.insideUnitSphere;
        randomPos.z = 0;
        randomPos.Normalize();
        randomPos = randomPos * minSpawnDistance + (Random.Range(0, maxSpawnDistance - minSpawnDistance) * randomPos);

        return randomPos + player.transform.position;*/
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
                levelGen.OutpostsInChunk[(Vector2Int)pos].RemoveAll(e => e == null);
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

    private IEnumerator extraHealth()
    {
        while (true)
        {
            ExtraEnemyHealthTime++;
            yield return new WaitForSeconds(60.0f);
        }
    }
}
