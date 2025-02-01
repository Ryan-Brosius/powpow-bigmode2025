using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting.InputSystem;
using System.Linq;

public enum OutpostType
{
    P,  // Green
    O,  // Blue
    W   // Red
}

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;

    [SerializeField] private List<Sprite> floorSprites = new();
    [SerializeField] private List<Sprite> decorSprites = new();
    [SerializeField] private List<Outpost> outposts = new();

    public const int CHUNK_SIZE = 8;
    private const int LOAD_DISTANCE = 32;

    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, OutpostType> outpostTypes = new Dictionary<Vector2Int, OutpostType>();
    private Dictionary<OutpostType, Vector2Int?> closestOutposts = new Dictionary<OutpostType, Vector2Int?>();
    private Dictionary<Vector2Int, List<GameObject>> outpostsInChunk = new Dictionary<Vector2Int, List<GameObject>>();

    //[HideInInspector] public Dictionary<OutpostType, Vector2Int?> ClosestOutposts { get { return closestOutposts; } }

    public UnityEvent<Dictionary<OutpostType, Vector2Int?>> OnOutpostsUpdated = new UnityEvent<Dictionary<OutpostType, Vector2Int?>>();

    private int worldSeed;
    private Vector2Int currentPlayerChunk;

    private static ChunkManager instance;
    public static ChunkManager Instance => instance;

    public Dictionary<OutpostType, Vector2Int?> ClosestOutposts => closestOutposts;
    public Dictionary<Vector2Int, List<GameObject>> OutpostsInChunk
    {
        get { return outpostsInChunk; }
        set { outpostsInChunk = value; }
    }

    System.Random random;

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
        //worldSeed = Random.Range(0,100);
        worldSeed = 0;
        random = new System.Random(worldSeed);

        //chunkCoord.x * 73856093 + chunkCoord.y * 19349663

        UpdateChunks(Vector2Int.zero);
        PlayerMovement.Instance.ChangeSpot.AddListener((Vector2 pos) => UpdateChunks(Vector2Int.RoundToInt(pos)));
    }

    private OutpostType DetermineOutpostType(Vector2Int chunkCoord)
    {
        //int hash = worldSeed;
        //hash = hash * 31 + chunkCoord.x;
        //hash = hash * 31 + chunkCoord.y;

        //int hash = (worldSeed + 31* chunkCoord.x + 31 * chunkCoord.y).GetHashCode();
        //return (OutpostType)((hash & 0x7fffffff) % 3);

        return (OutpostType)random.Next(0, 3);
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

        bool shouldHaveBlocks = random.Next(0, 10) == 0;
        var takenBlocks = new HashSet<Vector2Int>(); // Changed to HashSet for better performance

        if (shouldHaveBlocks)
        {
            OutpostsInChunk[chunkCoord] = new List<GameObject>();
            OutpostType outpostType = DetermineOutpostType(chunkCoord);
            outpostTypes[chunkCoord] = outpostType;

            // Define the grid size for hut placement
            int gridSize = 4; // Dividing the chunk into a 4x4 grid for hut placement
            var outPostGridTaken = new HashSet<Vector2Int>(); // Changed to HashSet
            int outpostCount = random.Next(1, 3); // Reduced max count to prevent overcrowding
            var outpostPrefab = outposts[Random.Range(0, outposts.Count - 1)].Prefab;

            for (int i = 0; i < outpostCount; i++)
            {
                int maxHutAttemps = 10;
                int hutAttempts = 0;

                while (hutAttempts < maxHutAttemps)
                {
                    // Generate grid position
                    int gridX = random.Next(0, gridSize);
                    int gridY = random.Next(0, gridSize);
                    var gridPos = new Vector2Int(gridX, gridY);

                    if (IsLargeAreaClear(gridPos, outPostGridTaken))
                    {
                        // Each grid cell is 2x2 units (CHUNK_SIZE / gridSize)
                        float cellSize = CHUNK_SIZE / (float)gridSize;
                        Vector3 localPos = new Vector3(
                            gridX * cellSize + cellSize / 2,
                            gridY * cellSize + cellSize / 2,
                            0
                        );

                        GameObject hut = Instantiate(outpostPrefab, chunkObject.transform);
                        hut.transform.localPosition = localPos;
                        OutpostsInChunk[chunkCoord].Add(hut);
                        //hut.GetComponent<SpriteRenderer>().color = OutpostToColor(outpostType);
                        //hut.GetComponent<SpriteRenderer>().sprite = outposts.Where(w => w.Type.Equals(outpostType)).ToList()[0].Sprite;

                        outPostGridTaken.Add(gridPos);

                        int blockX = Mathf.FloorToInt(localPos.x);
                        int blockY = Mathf.FloorToInt(localPos.y);
                        takenBlocks.Add(new Vector2Int(blockX, blockY));
                        takenBlocks.Add(new Vector2Int(blockX + 1, blockY));
                        takenBlocks.Add(new Vector2Int(blockX, blockY + 1));
                        takenBlocks.Add(new Vector2Int(blockX + 1, blockY + 1));
                        break;
                    }
                    hutAttempts++;
                }
            }
        }

        int floorTileCount = random.Next(1, 4); 
        for (int i = 0; i < floorTileCount; i++)
        {
            int maxAttempts = 10; 
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                Vector2Int newPos = new(random.Next(0, CHUNK_SIZE), random.Next(0, CHUNK_SIZE));
                if (!takenBlocks.Contains(newPos))
                {
                    Vector3 localPos = new Vector3(newPos.x, newPos.y, 0);

                    if (random.Next(0, 2) == 1)
                    {
                        GameObject floor = Instantiate(blockPrefab, chunkObject.transform);
                        floor.transform.localPosition = localPos;
                        floor.GetComponent<SpriteRenderer>().sprite = floorSprites[random.Next(0, floorSprites.Count)];
                        floor.GetComponent<SpriteRenderer>().sortingOrder = -1;
                        takenBlocks.Add(newPos);
                    }
                    else
                    {
                        GameObject decor = Instantiate(blockPrefab, chunkObject.transform);
                        decor.transform.localPosition = localPos;
                        decor.GetComponent<SpriteRenderer>().sprite = decorSprites[random.Next(0, decorSprites.Count)];
                        decor.GetComponent<SpriteRenderer>().sortingOrder = -1;
                        decor.AddComponent<Rigidbody2D>().isKinematic = true;
                        decor.AddComponent<BoxCollider2D>().size*=0.5f;
                        takenBlocks.Add(newPos);
                    }
                    break;
                }
                attempts++;
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

    private bool IsLargeAreaClear(Vector2Int gridPos, HashSet<Vector2Int> grid)
    {
        Vector2Int[] directions = {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1,  0),                        new Vector2Int(1,  0),
            new Vector2Int(-1,  1), new Vector2Int(0,  1), new Vector2Int(1,  1)
        };

        if (grid.Contains(gridPos))
            return false;

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = gridPos + dir;
            if (grid.Contains(neighborPos))
                return false;
        }

        return true;
    }
}

[System.Serializable]
struct Outpost
{
    public OutpostType Type;
    public GameObject Prefab;
}