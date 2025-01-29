public class PowData
{
    public int BulletPierce { get; set; }
    public int BulletDamage { get; set; }
    public int BulletsPerShot { get; set; }

    public override string ToString()
    {
        return $"Bullet Pierce (P): {BulletPierce}, Bullet Damage (O): {BulletDamage}, Bullets Per Shot (W): {BulletsPerShot}";
    }
}