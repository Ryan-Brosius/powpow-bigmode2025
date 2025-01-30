using UnityEngine;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;

    private const int CHUNK_SIZE = 8;
    private const int LOAD_DISTANCE = 166;

    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private int worldSeed;

    private void Start()
    {
        worldSeed = 12345;
        UpdateChunks(Vector2Int.zero);
        PlayerMovement.Instance.ChangeSpot.AddListener((Vector2 pos) => UpdateChunks(Vector2Int.RoundToInt(pos)));
    }

    public void UpdateChunks(Vector2Int playerPosition)
    {
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerPosition.x / (float)CHUNK_SIZE),
            Mathf.FloorToInt(playerPosition.y / (float)CHUNK_SIZE)
        );

        int loadDistanceInChunks = Mathf.CeilToInt(LOAD_DISTANCE / (float)CHUNK_SIZE);

        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

        for (int x = -loadDistanceInChunks; x <= loadDistanceInChunks; x++)
        {
            for (int y = -loadDistanceInChunks; y <= loadDistanceInChunks; y++)
            {
                Vector2Int checkChunk = playerChunk + new Vector2Int(x, y);

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
            Destroy(activeChunks[chunk]);
            activeChunks.Remove(chunk);
        }
    }

    private void GenerateChunk(Vector2Int chunkCoord)
    {
        GameObject chunkObject = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        chunkObject.transform.parent = transform;

        Vector2Int chunkWorldPos = chunkCoord * CHUNK_SIZE;
        chunkObject.transform.position = new Vector3(chunkWorldPos.x, chunkWorldPos.y, 0);

        System.Random random = new System.Random(worldSeed + chunkCoord.x * 73856093 + chunkCoord.y * 19349663);
        bool shouldHaveBlocks = random.Next(0, 10) == 0; // 10% chance

        if (shouldHaveBlocks)
        {
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
                }
            }
        }

        activeChunks.Add(chunkCoord, chunkObject);
    }

    public static Vector2Int WorldToChunkCoord(Vector2Int worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / (float)CHUNK_SIZE),
            Mathf.FloorToInt(worldPosition.y / (float)CHUNK_SIZE)
        );
    }
}