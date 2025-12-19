using UnityEngine;

namespace CLHoma.Upgrades
{
    [System.Serializable]
    public class BaseWeaponUpgrade : Upgrade<BaseWeaponUpgradeStage>
    {
        public override void Initialise()
        {

        }
    }

    [System.Serializable]
    public class BaseWeaponUpgradeStage : BaseUpgradeStage
    {
        [Header("Prefabs")]
        [SerializeField] GameObject weaponPrefab;
        public GameObject WeaponPrefab => weaponPrefab;

        [SerializeField] GameObject bulletPrefab;
        public GameObject BulletPrefab => bulletPrefab;

        [Header("Data")]
        [Range(1f, 5f)]
        [SerializeField] float factorDamage = 1f;
        public float FactorDamage => factorDamage;

        [SerializeField] float rangeRadius;
        public float RangeRadius => rangeRadius;

        [SerializeField, Tooltip("Shots Per Second")] float fireRate;
        public float FireRate => fireRate;

        [SerializeField] float spread;
        public float Spread => spread;

        [SerializeField] DuoInt bulletsPerShot = new DuoInt(1, 1);
        public DuoInt BulletsPerShot => bulletsPerShot;

        [SerializeField] DuoFloat bulletSpeed;
        public DuoFloat BulletSpeed => bulletSpeed;
    }
}