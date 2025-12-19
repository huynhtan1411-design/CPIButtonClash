using CLHoma.LevelSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CLHoma.Combat
{
    public class TeslaBulletBehavior : PlayerBulletBehavior
    {
        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Tesla Hit");
        private static readonly int PARTICLE_WALL_HIT_HASH = ParticlesController.GetHash("Tesla Wall Hit");

        [Space(5f)]
        [SerializeField] TrailRenderer trailRenderer;
        [SerializeField] Transform modelProjectile;

        private List<BaseEnemyBehavior> targets;

        private int targetsHitGoal;
        private int hitsPerformed;
        private float stunDuration;

        public void Initialise(float damage, float speed, ElementType elementType, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit, float stunDuration)
        {
            base.Initialise(damage, speed, elementType, currentTarget, autoDisableTime, autoDisableOnHit);

            this.stunDuration = stunDuration;
            trailRenderer.Clear();

            transform.localScale = Vector3.one;
            //transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.CubicIn).;

            hitsPerformed = 0;
            targets = ActiveRoom.GetAliveEnemies().OrderBy(e => Vector3.SqrMagnitude(e.transform.position - CharacterBehaviour.Transform.position)).ToList();
        }
        public void SetTargetsHitGoal(int goal)
        {
            targetsHitGoal = goal;
        }

        protected override void FixedUpdate()
        {
            if (targets.Count == 0)
            {
                DisableBullet();
                return;
            }

            if (hitsPerformed >= targetsHitGoal)
            {
                DisableBullet();
                return;
            }
            if (targets[0] == null || targets[0].IsDead)
            {
                targets.RemoveAt(0);
                return;
            }
            Vector3 targetDirection = targets[0].transform.position.SetY(1f) - transform.position;
            Vector3 rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0f);
            transform.rotation = Quaternion.LookRotation(rotationDirection);

            modelProjectile.Rotate(Vector3.up * 1280f * Time.fixedDeltaTime, Space.World);
            base.FixedUpdate();

            if (targets[0].IsDead)
            {
                targets.RemoveAt(0);
            }
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);

            trailRenderer.Clear();

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].IsDead || targets[i].Equals(baseEnemyBehavior))
                {
                    targets.RemoveAt(i);
                    i--;
                }
            }

            hitsPerformed++;

            // all hits after the first one deal 30% of damage
            if (hitsPerformed == 1)
            {
                damage *= 0.3f;
            }

            if (hitsPerformed >= targetsHitGoal || targets.Count == 0)
            {
                DisableBullet();
            }
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();

            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
            DisableBullet();
        }

        private void DisableBullet()
        {
            trailRenderer.Clear();
            gameObject.SetActive(false);
        }
    }
}