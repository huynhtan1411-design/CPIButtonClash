using CLHoma.DGTween;
using CLHoma.Upgrades;
using UnityEngine;
using System.Collections;

namespace CLHoma.Combat
{
    public class GeneralShootingBehavior : BaseGunBehavior
    {
        [SerializeField] LayerMask targetLayers;
        [SerializeField] float bulletDisableTime;
        [SerializeField] float delayBeforeShoot = 0.3f;

        private float attackDelay;
        private DuoFloat bulletSpeed;
        private float bulletSpreadAngle;

        private float nextShootTime;

        private Pool bulletPool;

        private TweenCase shootTweenCase;

        private ShotgunUpgrade upgrade;
        private int bulletsNumber;
        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponType weaponType, ElementType elementType)
        {
            base.Initialise(characterBehaviour, weaponType, elementType);

            upgrade = UpgradesController.GetUpgrade<ShotgunUpgrade>(weaponType);

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
            bulletSpreadAngle = stage.Spread;
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
            Vector3 shootDirectionNormalized = shootDirection.normalized;
            Vector3 forwardDirection = transform.forward;
            
            Vector3 horizontalShootDirection = new Vector3(shootDirectionNormalized.x, 0, shootDirectionNormalized.z).normalized;
            Vector3 horizontalForwardDirection = new Vector3(forwardDirection.x, 0, forwardDirection.z).normalized;
            if (Physics.Raycast(shootPoint.position, shootDirectionNormalized, out var hitInfo, 300f, targetLayers)
                && hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                if (Vector3.Angle(horizontalShootDirection, horizontalForwardDirection) < 40f)
                {
                    if (shootTweenCase != null && !shootTweenCase.isCompleted)
                        shootTweenCase.Kill();
                    float delay = attackDelay * (1 - characterBehaviour.Data.CooldownReduction);
                    if (delay < 0.3f)
                        delay = 0.3f;
                    nextShootTime = Time.timeSinceLevelLoad + delay;

                    StartCoroutine(ShootWithDelay(shootDirectionNormalized));

                    characterBehaviour.OnGunShooted();
                }
                else
                {
                }
            }
            else
            {
            }
        }

        private IEnumerator ShootWithDelay(Vector3 shootDirectionNormalized)
        {
            yield return new WaitForSeconds(delayBeforeShoot);
            for (int i = 0; i < bulletsNumber; i++)
            {
                float damage = GetDamage() *(1 + characterBehaviour.Data.AttackPower);
                float speed = bulletSpeed.Random()*(1+ characterBehaviour.Data.AttackPower);
                PlayerBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings().SetPosition(shootPoint.position).SetEulerRotation(characterBehaviour.transform.eulerAngles)).GetComponent<GeneralBulletBehavior>();
                bullet.Initialise(damage, speed, elementType,
                    characterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);
                
                if (i == 0)
                {
                    bullet.transform.rotation = Quaternion.LookRotation(shootDirectionNormalized);
                }
                if (i > 0)
                {
                    float ySpread = Random.Range(bulletSpreadAngle * -0.5f, bulletSpreadAngle * 0.5f);
                    float xSpread = Random.Range(bulletSpreadAngle * -0.5f, bulletSpreadAngle * 0.5f);
                    bullet.transform.Rotate(new Vector3(xSpread, ySpread, 0f));
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (characterBehaviour == null)
                return;

            if (characterBehaviour.ClosestEnemyBehaviour == null)
                return;

            Color defCol = Gizmos.color;
            Gizmos.color = Color.red;

            Vector3 shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position - shootPoint.position;

            Gizmos.DrawLine(shootPoint.position - shootDirection.normalized * 10f, characterBehaviour.ClosestEnemyBehaviour.transform.position);

            Gizmos.color = defCol;
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
            transform.SetParent(characterGraphics.ShootGunHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            bulletPool?.ReturnToPoolEverything();
        }
    }
}