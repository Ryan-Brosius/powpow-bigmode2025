using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Walk")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 10.0f;
    [Range(0.01f, 50f)] public float Acceleration = 5f;
    [Range(0.01f, 50f)] public float Deceleration = 20f;
}
