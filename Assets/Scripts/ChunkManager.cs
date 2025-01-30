using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEditor;

public enum OutpostType
{
    P,  // Green
    O,  // Blue
    W   // Red
}

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;

    public const int CHUNK_SIZE = 8;
    private const int LOAD_DISTANCE = 166;

    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, OutpostType> outpostTypes = new Dictionary<Vector2Int, OutpostType>();
    private Dictionary<OutpostType, Vector2Int?> closestOutposts = new Dictionary<OutpostType, Vector2Int?>();

    public UnityEvent<Dictionary<OutpostType, Vector2Int?>> OnOutpostsUpdated = new UnityEvent<Dictionary<OutpostType, Vector2Int?>>();

    private int worldSeed;
    private Vector2Int currentPlayerChunk;

    private static ChunkManager instance;
    public static ChunkManager Instance => instance;

    public Dictionary<OutpostType, Vector2Int?> ClosestOutposts => closestOutposts;

    private void Awake()
    {
        InitializeSingleton();
        InitializeOutpostTracking();
    }

    private void InitializeSingleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void InitializeOutpostTracking()
    {
        foreach (OutpostType type in System.Enum.GetValues(typeof(OutpostType)))
        {
            closestOutposts[type] = null;
        }
    }

    private void Start()
    {
        worldSeed = Random.Range(0,1000);
        UpdateChunks(Vector2Int.zero);
        PlayerMovement.Instance.ChangeSpot.AddListener((Vector2 pos) => UpdateChunks(Vector2Int.RoundToInt(pos)));
    }

    private OutpostType DetermineOutpostType(Vector2Int chunkCoord)
    {
        int hash = worldSeed;
        hash = hash * 31 + chunkCoord.x;
        hash = hash * 31 + chunkCoord.y;
        return (OutpostType)((hash & 0x7fffffff) % 3);
    }

    private void UpdateClosestOutposts(Vector2Int playerPosition)
    {
        Dictionary<OutpostType, Vector2Int?> previousClosest = new Dictionary<OutpostType, Vector2Int?>(closestOutposts);
        Dictionary<OutpostType, float> closestDistances = new Dictionary<OutpostType, float>();

        foreach (OutpostType type in System.Enum.GetValues(typeof(OutpostType)))
        {
            closestOutposts[type] = null;
            closestDistances[type] = float.MaxValue;
        }

        foreach (var outpost in outpostTypes)
        {
            Vector2Int outpostWorldPos = outpost.Key * CHUNK_SIZE + new Vector2Int(CHUNK_SIZE / 2, CHUNK_SIZE / 2);
            float distance = Vector2.Distance(playerPosition, outpostWorldPos);

            if (distance <= LOAD_DISTANCE && distance < closestDistances[outpost.Value])
            {
                closestOutposts[outpost.Value] = outpost.Key;
                closestDistances[outpost.Value] = distance;
            }
        }

        bool changed = false;
        foreach (var kvp in previousClosest)
        {
            if (!closestOutposts[kvp.Key].Equals(kvp.Value))
            {
                changed = true;
                break;
            }
        }

        if (changed)
        {
            OnOutpostsUpdated.Invoke(closestOutposts);
        }
    }

    public void UpdateChunks(Vector2Int playerPosition)
    {
        currentPlayerChunk = new Vector2Int(
            Mathf.FloorToInt(playerPosition.x / (float)CHUNK_SIZE),
            Mathf.FloorToInt(playerPosition.y / (float)CHUNK_SIZE)
        );

        int loadDistanceInChunks = Mathf.CeilToInt(LOAD_DISTANCE / (float)CHUNK_SIZE);
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

        for (int x = -loadDistanceInChunks; x <= loadDistanceInChunks; x++)
        {
            for (int y = -loadDistanceInChunks; y <= loadDistanceInChunks; y++)
            {
                Vector2Int checkChunk = currentPlayerChunk + new Vector2Int(x, y);
                Vector2Int chunkCenterWorld = checkChunk * CHUNK_SIZE + new Vector2Int(CHUNK_SIZE / 2, CHUNK_SIZE / 2);
                float distanceToPlayer = Vector2.Distance(playerPosition, chunkCenterWorld);

                if (distanceToPlayer <= LOAD_DISTANCE)
                {
                    chunksToKeep.Add(checkChunk);
                    if (!activeChunks.ContainsKey(checkChunk))
                    {
                        GenerateChunk(checkChunk);
                    }
                }
            }
        }

        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            if (!chunksToKeep.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunk in chunksToRemove)
        {
            if (outpostTypes.ContainsKey(chunk))
            {
                outpostTypes.Remove(chunk);
            }
            Destroy(activeChunks[chunk]);
            activeChunks.Remove(chunk);
        }

        UpdateClosestOutposts(playerPosition);
    }

    private void GenerateChunk(Vector2Int chunkCoord)
    {
        GameObject chunkObject = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        chunkObject.transform.parent = transform;

        Vector2Int chunkWorldPos = chunkCoord * CHUNK_SIZE;
        chunkObject.transform.position = new Vector3(chunkWorldPos.x, chunkWorldPos.y, 0);

        //

        System.Random random = new System.Random(worldSeed + chunkCoord.x * 73856093 + chunkCoord.y * 19349663);

        bool shouldHaveBlocks = random.Next(0, 10) == 0;


        if (shouldHaveBlocks)
        {
            OutpostType outpostType = DetermineOutpostType(chunkCoord);
            outpostTypes[chunkCoord] = outpostType;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Vector3 localPos = new Vector3(
                        x + (CHUNK_SIZE / 2f - 1),
                        y + (CHUNK_SIZE / 2f - 1),
                        0
                    );

                    GameObject block = Instantiate(blockPrefab, chunkObject.transform);
                    block.transform.localPosition = localPos;
                    block.GetComponent<SpriteRenderer>().color = OutpostToColor(outpostType);
                }
            }
        }

        activeChunks.Add(chunkCoord, chunkObject);
    }

    public Color OutpostToColor(OutpostType type)
    {
        switch (type)
        {
            case OutpostType.P:
                return Color.green;
            case OutpostType.O:
                return Color.blue;
            case OutpostType.W:
                return Color.red;
        }
        return Color.white;
    }
}