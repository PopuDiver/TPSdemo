/// <summary>
/// 步枪武器类
/// </summary>
public class RifleWeapon : WeaponBase
{

    public RifleWeapon() {
        
    }
    
    public RifleWeapon(int id, double shootCd, int maxBulletCount, int reloadCd, WeaponType weaponType, double flySpeed, double gravity, int damage)
        :base(id,shootCd,maxBulletCount,reloadCd,weaponType, flySpeed, gravity, damage)
    {
        
    }
}