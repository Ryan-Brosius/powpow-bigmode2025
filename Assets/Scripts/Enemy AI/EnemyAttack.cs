using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Stats")]
    [SerializeField] public GameObject bulletPrefab;
    [Range(0.2f, 5f)] public float fireRate;
    [Range(1f, 20f)] public int bulletCount;
    [Range(0f, 360f)] public float spreadAngle;
    [Tooltip("Slightly randomize shoot time")]
    [Range(0f, 1f)] public float fireRateVariation = 0.1f;

    public void PerformAttack(Transform firePoint, Transform target)
    {
        Vector3 direction = target.position - firePoint.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        for (int i = 0; i < bulletCount; i++)
        {
            float spread = spreadAngle * (i - (bulletCount - 1) / 2);
            Quaternion rotation = Quaternion.Euler(0, 0, angle + spread);
            Instantiate(bulletPrefab, firePoint.position, rotation);
        }
    }
}
