using UnityEngine;
using CLHoma.Combat;
using CLHoma;
using System.Collections;

namespace WD
{
    public class ArcherAllyBehavior : BaseAllyBehavior
    {
        [SerializeField] protected Transform projectileSpawnPoint;
        [SerializeField] protected GameObject projectilePrefab;
        protected Pool arrowPool;
        
        protected override void Awake()
        {
            base.Awake();
            if (projectilePrefab != null)
            {
                arrowPool = new Pool(new PoolSettings(projectilePrefab.name, projectilePrefab, 5, true));
            }
        }

        protected override void PerformAttackAction()
        {
            if (target == null || GameManager.IsPaused()) 
                return;

            ShootArrow();
        }

        protected virtual void ShootArrow()
        {
            if (GameManager.IsPaused() || projectilePrefab == null || projectileSpawnPoint == null) 
                return;

            Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
            GameObject projectileObj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            projectileObj.transform.LookAt(target.position);
            
            var projectile = projectileObj.GetComponent<PlayerBulletBehavior>();
            if (projectile != null)
            {
                projectile.Initialise(GetCurrentDamage(), 10f, ElementType.None, target.GetComponent<BaseEnemyBehavior>(), 2f, true, 0.1f);
                Audio_Manager.instance.play("sfx_eff_arrow_shoot");
            }
        }
    }
} 