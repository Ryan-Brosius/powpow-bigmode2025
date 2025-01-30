using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting.InputSystem;

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
    [SerializeField] private List<Outpost> outposts = new();

    public const int CHUNK_SIZE = 8;
    private const int LOAD_DISTANCE = 32;

    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, OutpostType> outpostTypes = new Dictionary<Vector2Int, OutpostType>();
    private Dictionary<OutpostType, Vector2Int?> closestOutposts = new Dictionary<OutpostType, Vector2Int?>();

    //[HideInInspector] public Dictionary<OutpostType, Vector2Int?> ClosestOutposts { get { return closestOutposts; } }

    public UnityEvent<Dictionary<OutpostType, Vector2Int?>> OnOutpostsUpdated = new UnityEvent<Dictionary<OutpostType, Vector2Int?>>();

    private int worldSeed;
    private Vector2Int currentPlayerChunk;

    private static ChunkManager instance;
    public static ChunkManager Instance => instance;

    public Dictionary<OutpostType, Vector2Int?> ClosestOutposts => closestOutposts;

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

        return (OutpostType)random.Next(0,3);
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


        bool shouldHaveBlocks = random.Next(0, 10) == 0;
        var takenBlocks = new List<Vector2>();

        if (shouldHaveBlocks)
        {
            OutpostType outpostType = DetermineOutpostType(chunkCoord);
            outpostTypes[chunkCoord] = outpostType;


            //because the huts are 2x2, we're doing multiples, and setting the hut in the center of the 4 squares
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Vector3 localPos = new Vector3(
                        x + (CHUNK_SIZE / 2f - 1),
                        y + (CHUNK_SIZE / 2f - 1),
                        0
                    );

                    ////if (random.Next(0, 2) == 0)
                    ////{
                    ////    GameObject floor = new GameObject();
                    ////    floor.AddComponent<SpriteRenderer>().sprite = floorSprites[random.Next(0, floorSprites.Count)];
                    ////    floor.transform.localPosition = localPos;

                    ////    GameObject block = Instantiate(blockPrefab, chunkObject.transform);
                    ////    block.transform.localPosition = localPos;
                    ////    block.GetComponent<SpriteRenderer>().color = OutpostToColor(outpostType);
                    ////}

                    GameObject block = Instantiate(blockPrefab, chunkObject.transform);
                    block.transform.localPosition = localPos;
                    block.GetComponent<SpriteRenderer>().color = OutpostToColor(outpostType);
                    takenBlocks.Add(new Vector2(x, y));
                }
            }
        }

        //filling in the floor, when not taken
        for (int i = 0; i < 2; i++)
        {
            Vector2 newPos = new(random.Next(0, 8), random.Next(0, 8));
            if (!takenBlocks.Contains(newPos))
            {
                Vector3 localPos = new Vector3(
                    newPos.x + (CHUNK_SIZE / 2f - 1),
                    newPos.y + (CHUNK_SIZE / 2f - 1),
                    0
                );

                GameObject floor = Instantiate(blockPrefab, chunkObject.transform);
                floor.transform.localPosition = localPos;
                floor.GetComponent<SpriteRenderer>().sprite = floorSprites[random.Next(0, floorSprites.Count)];
            }
            else
                i--;
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

[System.Serializable]
struct Outpost
{
    public OutpostType Type;
    public Sprite Sprite;
}