using CLHoma.DGTween;
using CLHoma.Upgrades;
using UnityEngine;

namespace CLHoma.Combat
{
    public class LightningMasterBehavior : BaseGunBehavior
    {
        [SerializeField] LayerMask targetLayers;
        [SerializeField] float explosionRadius;
        [SerializeField] DuoFloat bulletHeight;

        private float attackDelay;
        private DuoFloat bulletSpeed;

        private float nextShootTime;

        private Pool bulletPool;

        private TweenCase shootTweenCase;

        private float shootingRadius;
        private LavaLauncherUpgrade upgrade;
        private int bulletsNumber;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponType weaponType, ElementType elementType)
        {
            base.Initialise(characterBehaviour, weaponType, elementType);

            upgrade = UpgradesController.GetUpgrade<LavaLauncherUpgrade>(weaponType);
            GameObject bulletObj = (upgrade.GetCurrentStage(upgradeLevel) as BaseWeaponUpgradeStage).BulletPrefab;

            bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

            shootingRadius = characterBehaviour.EnemyDetector.DetectorRadius;

            RecalculateDamage();

        }

        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            var stage = upgrade.GetCurrentStage(upgradeLevel);

            factorDamage = stage.FactorDamage;
            attackDelay = 1f / stage.FireRate;
            bulletSpeed = stage.BulletSpeed;
            bulletsNumber = stage.BulletsPerShot.Random();
        }

        public override void GunUpdate()
        {
            if (!characterBehaviour.IsCloseEnemyFound || GameManager.Instance.IsGamePaused)
            {
                characterBehaviour.OnStopShoot();
                return;
            }
            if (nextShootTime >= Time.timeSinceLevelLoad)
                return;

            var shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position - shootPoint.position;
            Vector3 shootDirectionFlat = new Vector3(shootDirection.x, 0, shootDirection.z).normalized;
            Vector3 forwardFlat = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;

            if (Physics.Raycast(shootPoint.position, shootDirection.normalized, out var hitInfo, 300f, targetLayers)
                && hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                if (Vector3.Angle(shootDirectionFlat, forwardFlat) < 40f)
                {
                    if (shootTweenCase != null && !shootTweenCase.isCompleted)
                        shootTweenCase.Kill();

                    float delay = (attackDelay - attackDelay * characterBehaviour.Data.CooldownReduction);
                    if (delay < 0.3f)
                        delay = 0.3f;
                    nextShootTime = Time.timeSinceLevelLoad + delay;

                    var targets = characterBehaviour.GetRandomTargets(bulletsNumber);
                    for (int i = 0; i < bulletsNumber; i++)
                    {
                        LightningBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings().SetPosition(shootPoint.position).SetEulerRotation(shootPoint.eulerAngles)).GetComponent<LightningBulletBehavior>();
                        float bulletSpeedValue = bulletSpeed.Random() + bulletSpeed.Random()*characterBehaviour.Data.MoveSpeed;// bulletSpeed.Random() * PlayerController.StatsManager.projectileSpeed;
                        float damageValue = GetDamage() + GetDamage() * characterBehaviour.Data.AttackPower;
                        //bullet.Initialise(damageValue, 0, elementType, targets[i], -1f, false,
                          //  shootingRadius, characterBehaviour, bulletHeight, explosionRadius);
                    }
                    Audio_Manager.instance.play("sfx_eff_lightning_master");
                    characterBehaviour.OnGunShooted();
                }
            }
            else
            {
            }
        }

        public override void OnGunUnloaded()
        {
            if (bulletPool != null)
            {
                bulletPool.Clear();
                bulletPool = null;
            }
        }

        public override void PlaceGun(CharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.RocketHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            bulletPool.ReturnToPoolEverything();
        }
    }
}
