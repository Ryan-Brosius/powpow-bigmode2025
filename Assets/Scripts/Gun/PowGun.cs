using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowGun : MonoBehaviour
{
    [SerializeField] PowGunStats GunStats;

    [Header("Fire Information")]
    [SerializeField] private Transform firePoint;
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
        isShooting = true;

        foreach (var powData in bullets)
        {
            for (int i = 0; i < powData.BulletsPerShot; i++)
            {
                FireBullet(powData);
            }
            yield return new WaitForSeconds(GunStats.TimeToFireMag / bullets.Count);
        }

        isShooting = false;
        bullets = PowerGameState.Instance.PowData;
    }

    private void FireBullet(PowData powData)
    {
        GameObject bullet = Instantiate(GunStats.BulletPrefab, firePoint.position, firePoint.rotation);

        //bullet.transform.localScale = new Vector3(bullet.transform.localScale.x * powData.BulletSize, bullet.transform.localScale.y * powData.BulletSize, 1);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Damage = powData.BulletDamage;
            bulletScript.Pierce = powData.BulletPierce;
        }

        ApplyShotgunSpread(bullet, powData.BulletsPerShot);
    }


    private void ApplyShotgunSpread(GameObject bullet, int bulletsPerShot)
    {
        float spreadFactor = Mathf.Clamp(bulletsPerShot * GunStats.SpreadMultiplier, 0f, 1f);
        float spreadAngle = GunStats.MaxSpreadAngle * spreadFactor;

        float randomAngle = Random.Range(-spreadAngle, spreadAngle);
        bullet.transform.Rotate(0, 0, randomAngle);
    }
}
