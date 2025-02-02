using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PowGun : MonoBehaviour
{
    [SerializeField] PowGunStats GunStats;

    [Header("Fire Information")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float baseBulletSpeed = 6f;
    private List<PowData> bullets;
    private bool isShooting = false;

    private void Start()
    {
        bullets = PowerGameState.Instance.PowData;
    }

    private void Update()
    {
        if (!isShooting)
        {
            StartCoroutine(FireBullets());
        }
    }

    private IEnumerator FireBullets()
    {
        CameraController.Instance.ShakeCamera();

        isShooting = true;

        foreach (var powData in bullets)
        {
            // SPECIAL: Double Barrel
            if (powData.BulletPierce == 1 && powData.BulletDamage == 2 && powData.BulletsPerShot == 2)
            {
                powData.BulletDamage = 3;
                powData.BulletsPerShot = 3;
            }

                for (int i = 0; i < powData.BulletsPerShot; i++)
            {
                FireBullet(powData);

                // SPECIAL: Annihilator
                if (powData.BulletsPerShot >= 10)
                {
                    for (int j = 0; j < 4; ++j) FireBullet(powData);
                }
            }

            if (bullets.Count == 5)
            {
                yield return new WaitForSeconds(GunStats.TimeToFireMag / bullets.Count / 4f);

            }
            else if (powData.BulletPierce >= 10)
            {
                yield return new WaitForSeconds(GunStats.TimeToFireMag / bullets.Count / 10f);
            }
            else
            {
                yield return new WaitForSeconds(GunStats.TimeToFireMag / bullets.Count);
            }
        }

        isShooting = false;
        bullets = PowerGameState.Instance.PowData;
    }

    private void FireBullet(PowData powData)
    {
        GameObject bullet = Instantiate(GunStats.BulletPrefab, firePoint.position, firePoint.rotation);
        Destroy(bullet, 4f);

        //bullet.transform.localScale = new Vector3(bullet.transform.localScale.x * powData.BulletSize, bullet.transform.localScale.y * powData.BulletSize, 1);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Damage = powData.BulletDamage;
            bulletScript.Pierce = powData.BulletPierce;
            bulletScript.speed = baseBulletSpeed + powData.BulletPierce;
        }
        // SPECIAL: Railgun
        if (powData.BulletPierce >= 10) bulletScript.speed = 100f;
        // SPECIAL: Quantum Cannon
        if (powData.BulletDamage >= 10)
        {
            bulletScript.timeUntilDeath = 10f;
            bullet.transform.localScale *= 5;
            bulletScript.Pierce = 99;
            bulletScript.Damage = 20;
        }
        // SPECIAL: Double Barrel
        if (powData.BulletPierce == 1 && powData.BulletDamage == 3 && powData.BulletsPerShot == 3)
        {
            bulletScript.speed = baseBulletSpeed = 16f;
            bulletScript.StartCoroutine(bulletScript.DeathTimer(0.3f));
        }
        // SPECIAL: Sonic Shot
        if (powData.BulletPierce == 2 && powData.BulletDamage == 2 && powData.BulletsPerShot == 1)
        {
            bulletScript.speed = 50f;
            bulletScript.Pierce = 5;
        }
        if (powData.BulletPierce != 13) ApplyShotgunSpread(bullet, bullets.Count == 5 ? 4 : powData.BulletsPerShot);
    }


    private void ApplyShotgunSpread(GameObject bullet, int bulletsPerShot)
    {
        float spreadFactor = Mathf.Clamp(bulletsPerShot * GunStats.SpreadMultiplier, 0f, 1f);
        float spreadAngle = GunStats.MaxSpreadAngle * spreadFactor;
        if (bulletsPerShot >= 10) spreadAngle = 360f;

        float randomAngle = Random.Range(-spreadAngle, spreadAngle);
        bullet.transform.Rotate(0, 0, randomAngle);
    }
}
