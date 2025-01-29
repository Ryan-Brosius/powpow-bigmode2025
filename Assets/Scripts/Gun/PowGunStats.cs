using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PowGun Stats")]
public class PowGunStats : ScriptableObject
{
    [Header("General Fire Stats")]
    [SerializeField] public GameObject BulletPrefab;
    [Range(0.1f, 2f)] public float TimeToFireMag = 1.0f;

    [Header("Shotgun Stats")]
    [Range(0f, 30f)] public float MaxSpreadAngle = 20f;
    [Range(0f, 1f)] public float SpreadMultiplier = .2f;
}
