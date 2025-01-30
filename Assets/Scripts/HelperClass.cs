using System.Collections.Generic;
using UnityEngine;

public static class HelperClass
{
    public static Vector2 RotateVector(Vector2 v, float delta)
    {
        delta *= Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }

    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random.Range(i, count);
            (ts[r], ts[i]) = (ts[i], ts[r]);
        }
    }

    public static Texture2D TextureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newText = new((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
            return sprite.texture;
    }

    public static void DebugColored(string message) {
        DebugColored(message, HelperDebugColors.RED);
    }
    public static void DebugColored(string message, HelperDebugColors color)
    {
        string hexColor = GetColorHexFromEnum(color);
        Debug.Log($"<color={hexColor}>{message}</color>");
    }

    private static string GetColorHexFromEnum(HelperDebugColors color)
    {
        return color switch
        {
            HelperDebugColors.RED => "#FF0000",
            HelperDebugColors.YELLOW => "#FFFF00",
            HelperDebugColors.GREEN => "#00FF00",
            HelperDebugColors.BLUE => "#0000FF",
            _ => "#FFFFFF"
        };
    }
}

public enum HelperDebugColors
{
    RED,
    YELLOW,
    GREEN,
    BLUE,
}
