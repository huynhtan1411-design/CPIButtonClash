using UnityEngine;
using System.Collections;

namespace CLHoma.Combat
{
    public class RangedEnemyBehavior : BaseEnemyBehavior
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private EnemyBulletBehavior bulletPrefab;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private float bulletDestroyDistance = 20f;
        [SerializeField] private float attackDelay = 1.5f;
        [SerializeField] private float bulletSpawnDelay = 0.3f;

        private float nextShootTime;
        private Pool bulletPool;

        public override void Initialise()
        {
            base.Initialise();
            attackAnimationDelay = bulletSpawnDelay;
            bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab.gameObject, 10, true));
        }

        protected override void PerformAttackAction()
        {
            if (target == null) return;

            StartCoroutine(ShootAndDamageSequence());
        }

        private IEnumerator ShootAndDamageSequence()
        {
            EnemyBulletBehavior bullet = bulletPool
                .GetPooledObject(new PooledObjectSettings()
                .SetPosition(firePoint.position)
                .SetEulerRotation(Vector3.zero))
                .GetComponent<EnemyBulletBehavior>();

            Vector3 direction = target.position - firePoint.position;
            direction.x = 0;
            direction.y = 0;
            float distanceToTarget = direction.magnitude;
            float bulletTravelTime = distanceToTarget / bulletSpeed + 0.2f;

            bullet.Initialise(GetCurrentDamage(), bulletSpeed, bulletDestroyDistance, target);
            bullet.transform.rotation = Quaternion.LookRotation(direction);

            yield return new WaitForSeconds(bulletTravelTime);

            if (target != null)
            {
                var enemyAI = GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.PerformAttack();
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (bulletPool != null)
            {
                bulletPool.Clear();
                bulletPool = null;
            }
        }
    }
}
