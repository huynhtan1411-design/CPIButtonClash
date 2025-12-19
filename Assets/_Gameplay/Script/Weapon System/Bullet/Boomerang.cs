using UnityEngine;

namespace CLHoma.Combat
{
    [RequireComponent(typeof(Collider))]
    public abstract class Boomerang : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] protected float speed = 12f;
        [SerializeField] protected float maxDistance = 12f;
        [SerializeField] protected float sideOffset = 4f;

        protected float damage;
        protected ElementType elementType;

        protected Vector3 startPos;
        protected Vector3 endPos;
        protected Vector3 forwardDir;
        protected float sideSign;

        protected float t;
        protected bool returning;

        private Vector3 lastPos;

        // =========================
        // INIT
        // =========================
        public virtual void Initialise(
            float damage,
            float speed,
            ElementType element,
            Vector3 castDirection,
            float sideSign
        )
        {
            this.damage = damage;
            this.speed = speed;
            this.elementType = element;
            this.sideSign = sideSign;

            startPos = transform.position;
            forwardDir = castDirection.normalized;
            endPos = startPos + forwardDir * maxDistance;

            t = 0f;
            returning = false;

            lastPos = startPos;
        }

        // =========================
        // UPDATE
        // =========================
        protected virtual void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime * speed / maxDistance;

            if (!returning)
            {
                t += delta;
                if (t >= 1f)
                {
                    t = 1f;
                    returning = true;
                }
            }
            else
            {
                t -= delta;
                if (t <= 0f)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }

            Vector3 basePos = Vector3.Lerp(startPos, endPos, t);

            float curve = Mathf.Sin(t * Mathf.PI);
            Vector3 sideDir = Vector3.Cross(Vector3.up, forwardDir).normalized;

            Vector3 finalPos =
                basePos +
                sideDir * curve * sideOffset * sideSign;

            transform.position = finalPos;

            Vector3 velocity = finalPos - lastPos;
            if (velocity.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(velocity.normalized);

            lastPos = finalPos;
        }

        // =========================
        // DAMAGE
        // =========================
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != PhysicsHelper.LAYER_ENEMY)
                return;

            BaseEnemyBehavior enemy = other.GetComponent<BaseEnemyBehavior>();
            if (enemy == null || enemy.IsDead)
                return;

            (float dmg, HitType hitType) result =
                Utils.CalculateElementalDamage(
                    damage,
                    elementType,
                    enemy.ElementType,
                    WD.GameManager.Instance.GameConfig.resistanceFactor,
                    WD.GameManager.Instance.GameConfig.weaknessFactor
                );

            enemy.TakeDamage(result.dmg, transform.position, transform.forward, result.hitType);
            OnEnemyHitted(enemy);
        }

        protected virtual void OnEnemyHitted(BaseEnemyBehavior enemy) { }
    }
}
