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
    [Tooltip("Distance between shots without angle (recommended)")]
    [Range(0f, 10f)] public float distanceBetweenShots = 0f;
    [Tooltip("Slightly randomize shoot time")]
    [Range(0f, 1f)] public float fireRateVariation = 0.1f;
    [Tooltip("Amount of shots that are in a row")]
    [Range(0f, 5f)] public int extraShotsInARow = 0;
    [Tooltip("Distance in time between extra shots")]
    [Range(0f, 1f)] public float extraShotFireRate = 0.1f;

    public void PerformAttack(Transform firePoint, Transform target)
    {
        Vector3 direction = target.position - firePoint.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        for (int i = 0; i < bulletCount; i++)
        {
            float spread = spreadAngle * (i - (bulletCount - 1) / 2);
            Quaternion rotation = Quaternion.Euler(0, 0, angle + spread);


            Vector3 extraDistance = bulletCount <= 1 ? Vector3.zero : Mathf.Lerp(-distanceBetweenShots / 2, distanceBetweenShots / 2, i / (bulletCount - 1)) * (rotation * Vector3.up);

            Instantiate(bulletPrefab, firePoint.position + extraDistance, rotation);
        }
        if (extraShotsInARow > 0) StartCoroutine(shootExtra(firePoint, angle));
    }

    private IEnumerator shootExtra(Transform firePoint, float angle)
    {
        for (int i = 0; i < extraShotsInARow; ++i)
        {
            yield return new WaitForSeconds(extraShotFireRate);
            for (int j = 0; j < bulletCount; j++)
            {
                float spread = spreadAngle * (j - (bulletCount - 1) / 2);
                Quaternion rotation = Quaternion.Euler(0, 0, angle + spread);

                Vector3 extraDistance = bulletCount <= 1 ? Vector3.zero : Mathf.Lerp(-distanceBetweenShots / 2, distanceBetweenShots / 2, i / (bulletCount - 1)) * (rotation * Vector3.up);

                Instantiate(bulletPrefab, firePoint.position + extraDistance, rotation);
            }
        }
    }
}
