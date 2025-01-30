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

            //idk man
            newCompass.GetComponent<SpriteRenderer>().flipX = true;
            newCompass.GetComponent<SpriteRenderer>().flipY = true;
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
                compasses[kvp.Key].GO.transform.localPosition = CompassHelper.NormalizeToCardinal(direction);

                //compasses[kvp.Key].GO.GetComponent<SpriteRenderer>().sprite = GetDirectionalSprite(direction);
                compasses[kvp.Key].GO.GetComponent<SpriteRenderer>().sprite = compassSprites[CompassHelper.GetCardinalIndex(direction)];

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
    public List<Sprite> Sprites; //this isnt needed for now lol

    public Compass(OutpostType type, GameObject go, List<Sprite> sprites)
    {
        Type = type;
        GO = go;
        Sprites = sprites;
    }
}

public static class CompassHelper
{
    public static Vector2 NormalizeToCardinal(this Vector2 vector)
    {
        if (vector.magnitude < 0.001f)
            return Vector2.zero;

        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

        //i dont know why this fixes everything
        //angle += 180;

        if (angle < 0) angle += 360f;

        if (angle > 337.5f || angle <= 22.5f)         // Right (0)
            return Vector2.right;
        else if (angle > 22.5f && angle <= 67.5f)     // Right-Up (1)
            return new Vector2(1, 1).normalized;
        else if (angle > 67.5f && angle <= 112.5f)    // Up (2)
            return Vector2.up;
        else if (angle > 112.5f && angle <= 157.5f)   // Left-Up (3)
            return new Vector2(-1, 1).normalized;
        else if (angle > 157.5f && angle <= 202.5f)   // Left (4)
            return Vector2.left;
        else if (angle > 202.5f && angle <= 247.5f)   // Left-Down (5)
            return new Vector2(-1, -1).normalized;
        else if (angle > 247.5f && angle <= 292.5f)   // Down (6)
            return Vector2.down;
        else                                          // Right-Down (7)
            return new Vector2(1, -1).normalized;
    }

    public static int GetCardinalIndex(this Vector2 vector)
    {
        if (vector.magnitude < 0.001f)
            return 0; 

        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

        //i dont know why this fixes everything
        angle += 90;
        if (angle < 0) angle += 360f;

        if (angle > 337.5f || angle <= 22.5f) return 0;  // Right
        else if (angle > 22.5f && angle <= 67.5f) return 1;  // Right-Up
        else if (angle > 67.5f && angle <= 112.5f) return 2;  // Up
        else if (angle > 112.5f && angle <= 157.5f) return 3;  // Left-Up
        else if (angle > 157.5f && angle <= 202.5f) return 4;  // Left
        else if (angle > 202.5f && angle <= 247.5f) return 5;  // Left-Down
        else if (angle > 247.5f && angle <= 292.5f) return 6;  // Down
        else return 7;  // Right-Down
    }
}