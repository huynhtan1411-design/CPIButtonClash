using CLHoma.DGTween;
using CLHoma.Upgrades;
using System.Collections.Generic;
using UnityEngine;

namespace CLHoma.Combat
{
    public class CrowbowBehavior : BaseGunBehavior
    {
        [SerializeField] LayerMask targetLayers;
        [SerializeField] float bulletDisableTime;

        [Space]
        [SerializeField] float fireRotationSpeed;

        [Space]
        [SerializeField] List<float> bulletStreamAngles;

        private float spread;
        private float attackDelay;
        private DuoFloat bulletSpeed;

        private float nextShootTime;

        private Pool bulletPool;

        private Vector3 shootDirection;

        private MinigunUpgrade upgrade;

        private TweenCase shootTweenCase;

        private int bulletsNumber;
        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponType weaponType, ElementType elementType)
        {
            base.Initialise(characterBehaviour, weaponType, elementType);

            upgrade = UpgradesController.GetUpgrade<MinigunUpgrade>(weaponType);

            GameObject bulletObj = (upgrade.GetCurrentStage(upgradeLevel) as BaseWeaponUpgradeStage).BulletPrefab;
            bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

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
            spread = stage.Spread;
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

            shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position - shootPoint.position;

            Vector3 shootDirectionFlat = new Vector3(shootDirection.x, 0, shootDirection.z).normalized;
            Vector3 forwardFlat = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;

            if (Physics.Raycast(shootPoint.position, shootDirection.normalized, out var hitInfo, 300f, targetLayers)
                && hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                if (Vector3.Angle(shootDirectionFlat, forwardFlat) < 40f)
                {
                    if (shootTweenCase != null && !shootTweenCase.isCompleted)
                        shootTweenCase.Kill();

                    //shootTweenCase = transform.DOLocalMoveZ(-0.0825f, attackDelay * 0.3f).OnComplete(delegate
                    //{
                    //    shootTweenCase = transform.DOLocalMoveZ(0, attackDelay * 0.6f);
                    //});

                    float delay = (attackDelay - attackDelay * characterBehaviour.Data.CooldownReduction);
                    if (delay < 0.3f)
                        delay = 0.3f;
                    nextShootTime = Time.timeSinceLevelLoad + delay;

                    if (bulletStreamAngles.IsNullOrEmpty())
                    {
                        bulletStreamAngles = new List<float> { 0 };
                    }
                    for (int k = 0; k < bulletsNumber; k++)
                    {
                        for (int i = 0; i < bulletStreamAngles.Count; i++)
                        {
                            var streamAngle = bulletStreamAngles[i];

                            Vector3 bulletDirection = Quaternion.Euler(0, Random.Range(-spread, spread) + streamAngle, 0) * shootDirection.normalized;

                            PlayerBulletBehavior bullet = bulletPool
                                .GetPooledObject(new PooledObjectSettings()
                                .SetPosition(shootPoint.position)
                                .SetEulerRotation(Quaternion.LookRotation(bulletDirection).eulerAngles))
                                .GetComponent<PlayerBulletBehavior>();

                            float bulletSpeedValue = bulletSpeed.Random() + bulletSpeed.Random() * characterBehaviour.Data.MoveSpeed;//bulletSpeed.Random() * PlayerController.StatsManager.projectileSpeed;
                            float bulletDisableTimeValue = bulletDisableTime * PlayerController.StatsManager.skillDuration;
                            float damageValue = GetDamage() + GetDamage() * characterBehaviour.Data.AttackPower;
                            bullet.Initialise(damageValue,
                                bulletSpeedValue,
                                elementType,
                                characterBehaviour.ClosestEnemyBehaviour,
                                bulletDisableTimeValue);
                        }
                    }
                    characterBehaviour.OnGunShooted();
                }
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
            transform.SetParent(characterGraphics.MinigunHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            bulletPool.ReturnToPoolEverything();
        }
    }
}