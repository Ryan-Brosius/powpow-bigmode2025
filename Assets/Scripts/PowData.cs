public class PowData
{
    public int BulletSize { get; set; }
    public int BulletDamage { get; set; }
    public int BulletsPerShot { get; set; }

    public override string ToString()
    {
        return $"Bullet Size (P): {BulletSize}, Bullet Damage (O): {BulletDamage}, Bullets Per Shot (W): {BulletsPerShot}";
    }
}