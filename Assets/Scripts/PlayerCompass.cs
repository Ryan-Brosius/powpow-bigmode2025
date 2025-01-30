using UnityEngine;
using System.Collections.Generic;

public class PlayerCompass : MonoBehaviour
{
    [SerializeField] private List<Sprite> compassSprites = new();
    private Dictionary<OutpostType, Compass> compasses = new();
    private ChunkManager chunkManager;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = PlayerMovement.Instance.transform;
        chunkManager = ChunkManager.Instance;

        foreach (OutpostType type in System.Enum.GetValues(typeof(OutpostType)))
        {
            var newCompass = new GameObject($"Compass_{type}");
            newCompass.AddComponent<SpriteRenderer>().sprite = compassSprites[0];
            newCompass.transform.parent = transform;
            compasses[type] = new Compass(type, newCompass, compassSprites);
        }

        chunkManager.OnOutpostsUpdated.AddListener(UpdateCompassPositions);
    }

    void UpdateCompassPositions(Dictionary<OutpostType, Vector2Int?> closestOutposts)
    {
        var playerPos = playerTransform.position;
        foreach (var kvp in closestOutposts)
        {
            if (kvp.Value.HasValue)
            {
                Vector2Int outpostChunkPos = kvp.Value.Value;
                Vector2 outpostWorldPos = new Vector2(
                    outpostChunkPos.x * ChunkManager.CHUNK_SIZE + ChunkManager.CHUNK_SIZE / 2,
                    outpostChunkPos.y * ChunkManager.CHUNK_SIZE + ChunkManager.CHUNK_SIZE / 2
                );

                Vector2 direction = (outpostWorldPos - (Vector2)playerPos).normalized;
                compasses[kvp.Key].GO.transform.localPosition = (Vector2)playerTransform.position + direction;
                compasses[kvp.Key].GO.SetActive(true);
            }
            else
            {
                compasses[kvp.Key].GO.SetActive(false);
            }
        }
    }
}

public struct Compass
{
    public OutpostType Type;
    public GameObject GO;
    public List<Sprite> Sprites;

    public Compass(OutpostType type, GameObject go, List<Sprite> sprites)
    {
        Type = type;
        GO = go;
        Sprites = sprites;
    }
}