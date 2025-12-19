using UnityEngine;
using CLHoma.Combat;
using CLHoma;
namespace WD
{
    public class SpecialTowerBehavior : TowerBehavior
    {
        public enum SpecialTowerType
        {
            Lightning,
            Cannon, 
        }
        public SpecialTowerType specialTowerType;

        private float damageExplosion;
        private float radiusExplosion;

        protected override void ApplyLevelStats()
        {
            base.ApplyLevelStats();
            var levelData = buildingConfig.levelConfigs[currentLevel - 1];
            damageExplosion = levelData.damageExplosion;
            radiusExplosion = levelData.radiusExplosion;
        }
        protected override void AttackEnemy(BaseEnemyBehavior enemy)
        {
            if (GameManager.IsPaused() || attackCooldown > 0)
                return;

            if (buildingGraphics != null)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                buildingGraphics.StartTrackingTarget(direction, delegate
                {
                    ShootProjectile(enemy);
                });
            }
        }

        protected override void ShootProjectile(BaseEnemyBehavior target)
        {
            if (projectilePrefab == null) return;
            Vector3 spawnPosition = projectileSpawnPoint.position;
            GameObject projectileObj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            if (specialTowerType  == SpecialTowerType.Lightning)
            {
                LightningBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings().SetEulerRotation(target.transform.eulerAngles)).GetComponent<LightningBulletBehavior>();
                bullet.Initialise(currentAttackDamage, 0, ElementType.None, target, -1f, false, radiusExplosion, damageExplosion);
                Audio_Manager.instance.play("sfx_eff_lightning_master");
            }
            else
            {
                LavaBulletBehavior bullet = bulletPool.GetPooledObject(new PooledObjectSettings().SetPosition(projectileSpawnPoint.position).SetEulerRotation(projectileSpawnPoint.eulerAngles)).GetComponent<LavaBulletBehavior>();
                bullet.Initialise(currentAttackDamage, 10f, ElementType.None, target, -1f, false, radiusExplosion, damageExplosion);
                Audio_Manager.instance.play("Bazoka_SFX");
            }
        }
    }
} 