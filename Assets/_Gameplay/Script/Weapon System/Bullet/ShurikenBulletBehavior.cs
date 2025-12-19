using UnityEngine;
namespace CLHoma.Combat
{
    public class ShurikenBulletBehavior : PlayerBulletBehavior
    {
        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Tesla Hit");
        private static readonly int PARTICLE_WALL_HIT_HASH = ParticlesController.GetHash("Tesla Wall Hit");

        [SerializeField] TrailRenderer trailRenderer;
        [SerializeField] Transform modelProjectile;

        private Vector3 startPosition;
        private bool isReturning;

        private Vector3 targetPos;
        private float flightTimer;
        private const float MAX_FLIGHT_TIME = 2f;

        private bool playedSoundHit = false;
        public void Initialise(float damage, float speed, ElementType element, BaseEnemyBehavior target, float autoDisableTime, bool autoDisableOnHit, float stunDuration)
        {
            base.Initialise(damage, speed, element, target, autoDisableTime, autoDisableOnHit);
            trailRenderer.Clear();

            startPosition = transform.position;
            transform.localScale = Vector3.one * 2f;

            targetPos = Utils.GetRandomPositionAround(target.transform.position, 0.5f);
            isReturning = false;
            flightTimer = 0f;

            playedSoundHit = false;
        }

        protected override void FixedUpdate()
        {
            modelProjectile.Rotate(Vector3.up * 900f * Time.fixedDeltaTime, Space.World);
            if (isReturning)
            {
                float step = speed * Time.fixedDeltaTime;
                Vector3 returnPosition = startPosition;
                transform.position = Vector3.MoveTowards(transform.position, returnPosition, step);

                if (Vector3.Distance(transform.position, returnPosition) < 0.1f)
                {
                    DisableBullet();
                }
                return;
            }

            flightTimer += Time.fixedDeltaTime;
            
            if (flightTimer >= MAX_FLIGHT_TIME)
            {
                isReturning = true;
                return;
            }

            Vector3 targetDirection = targetPos - transform.position;
            transform.position += targetDirection.normalized * speed * Time.fixedDeltaTime;

            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                isReturning = true;
                return;
            }
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior enemy)
        {
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);

            if (!playedSoundHit)
            {
                Audio_Manager.instance.play("sfx_eff_flesh_hit");
                playedSoundHit = true;
            }
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();
            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
        }
        private void DisableBullet()
        {
            trailRenderer.Clear();
            gameObject.SetActive(false);
        }
    }
}
