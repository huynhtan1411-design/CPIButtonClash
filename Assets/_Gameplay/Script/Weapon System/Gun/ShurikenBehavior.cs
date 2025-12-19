using CLHoma.DGTween;
using CLHoma.Upgrades;
using System.Collections.Generic;
using UnityEngine;

namespace CLHoma.Combat
{
    public class ShurikenBehavior : BaseGunBehavior
    {
        [SerializeField] LayerMask targetLayers;
        [SerializeField] float chargeDuration;
        private DuoFloat bulletSpeed;
        [SerializeField] DuoInt targetsHitGoal;
        [SerializeField] float stunDuration = 0.2f;

        private Pool bulletPool;

        private TweenCase shootTweenCase;
        private Vector3 shootDirection;

        private TeslaGunUpgrade upgrade;

        private float spread;
        private int bulletsNumber;


        [SerializeField] private List<GameObject> shurikenObj = new List<GameObject>();
        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponType weaponType, ElementType elementType)
        {
            base.Initialise(characterBehaviour, weaponType, elementType);

            upgrade = UpgradesController.GetUpgrade<TeslaGunUpgrade>(weaponType);
            GameObject bulletObj = (upgrade.GetCurrentStage(upgradeLevel) as BaseWeaponUpgradeStage).BulletPrefab;

            bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

            RecalculateDamage();
        }

        private bool HasShurikenInField()
        {
            foreach (var obj in shurikenObj)
            {
                if (obj.activeSelf)
                {
                    return true;
                }
            }
            shurikenObj.Clear();
            return false;
        }
        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            var stage = upgrade.GetCurrentStage(upgradeLevel);
            factorDamage = stage.FactorDamage;
            bulletSpeed = stage.BulletSpeed;
            spread = stage.Spread;
            bulletsNumber = stage.BulletsPerShot.Random();
        }

        public override void GunUpdate()
        {
            if (!characterBehaviour.IsCloseEnemyFound || GameManager.Instance.IsGamePaused)
            {
                characterBehaviour.OnStopShoot();
                return;
            }
            if (HasShurikenInField())
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


                    var targets = characterBehaviour.GetRandomTargets(bulletsNumber);
                    for (int k = 0; k < bulletsNumber; k++)
                    {
                        float bulletSpeedValue = bulletSpeed.Random() + bulletSpeed.Random() * characterBehaviour.Data.MoveSpeed;// bulletSpeed.Random() * PlayerController.StatsManager.projectileSpeed;
                        float damageValue = GetDamage() + GetDamage() * characterBehaviour.Data.AttackPower;
                        ShurikenBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings().
                            SetPosition(shootPoint.position).
                            SetEulerRotation(characterBehaviour.transform.eulerAngles)).
                            GetComponent<ShurikenBulletBehavior>();
                        bullet.Initialise(damageValue,
                            bulletSpeedValue,
                            elementType,
                            targets[k], 5f, false, stunDuration);

                        shurikenObj.Add(bullet.gameObject);
                    }
                    Audio_Manager.instance.play("sfx_eff_shuriken_shoot");
                    characterBehaviour.OnGunShooted();
                }
            }
        }


        public override void OnGunUnloaded()
        {
            // Destroy bullets pool
            if (bulletPool != null)
            {
                bulletPool.Clear();
                bulletPool = null;
            }
        }

        public override void PlaceGun(CharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.TeslaHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            bulletPool.ReturnToPoolEverything();
        }
    }
}