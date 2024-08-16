/// <summary>
/// 武器基类
/// </summary>
public class WeaponBase {
    // 武器ID
    public int ID;
    // 武器攻击力
    public int Damage;
    // 武器攻击 CD
    public double ShootCD;
    // 武器当前弹量
    public int BulletCount;
    // 武器最大弹量
    public int MaxBulletCount;
    // 换弹冷却
    public float ReloadCD;
    // 武器类型
    public WeaponType WeaponType;
    // 子弹飞行速度
    public double BulletFlySpeed;
    // 子弹所受重力
    public double BulletGravity;
    
    
    public WeaponBase() {
        this.BulletCount = this.MaxBulletCount;
    }
    
    public WeaponBase(int id, double shootCd, int maxBulletCount, int reloadCd, WeaponType weaponType, double bulletFlySpeed, double bulletGravity, int damage) {
        this.ID = id;
        this.ShootCD = shootCd;
        this.BulletCount = maxBulletCount;
        this.MaxBulletCount = maxBulletCount;
        this.ReloadCD = reloadCd;
        this.WeaponType = weaponType;
        this.BulletFlySpeed = bulletFlySpeed;
        this.BulletGravity = bulletGravity;
        this.Damage = damage;
    }
}