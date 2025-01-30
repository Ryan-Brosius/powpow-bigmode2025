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
            newCompass.GetComponent<SpriteRenderer>().color = chunkManager.OutpostToColor(type);
        }

        //chunkManager.OnOutpostsUpdated.AddListener(UpdateCompassPositions);
        PlayerMovement.Instance.ChangeSpot.AddListener((_)=>UpdateCompassPositions(chunkManager.ClosestOutposts));
    }

    private void Update()
    {
        transform.position = playerTransform.position;
    }
    //void UpdateClosestOutposts() => UpdateClosestOutposts()
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
                compasses[kvp.Key].GO.transform.localPosition = direction;

                compasses[kvp.Key].GO.GetComponent<SpriteRenderer>().sprite = GetDirectionalSprite(direction);

                compasses[kvp.Key].GO.SetActive(true);
            }
            else
            {
                compasses[kvp.Key].GO.SetActive(false);
            }
        }
    }

    Sprite GetDirectionalSprite(Vector2 dir)
    {
        //float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;

        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360f;

        int spriteIndex;

        if (angle > 337.5f || angle <= 22.5f)  // North (0)
            spriteIndex = 0;
        else if (angle > 22.5f && angle <= 67.5f)  // Northeast (1)
            spriteIndex = 1;
        else if (angle > 67.5f && angle <= 112.5f)  // East (2)
            spriteIndex = 2;
        else if (angle > 112.5f && angle <= 157.5f)  // Southeast (3)
            spriteIndex = 3;
        else if (angle > 157.5f && angle <= 202.5f)  // South (4)
            spriteIndex = 4;
        else if (angle > 202.5f && angle <= 247.5f)  // Southwest (5)
            spriteIndex = 5;
        else if (angle > 247.5f && angle <= 292.5f)  // West (6)
            spriteIndex = 6;
        else  // Northwest (7) - 292.5f to 337.5f
            spriteIndex = 7;

        //if (spriteIndex < directionalSprites.Count)
        //{
        //    sr.sprite = directionalSprites[spriteIndex];
        //}

        return compassSprites[spriteIndex];
    }
}

public struct Compass
{
    public OutpostType Type;
    public GameObject GO;
    public List<Sprite> Sprites; //this isnt needed for now lol

    public Compass(OutpostType type, GameObject go, List<Sprite> sprites)
    {
        Type = type;
        GO = go;
        Sprites = sprites;
    }
}