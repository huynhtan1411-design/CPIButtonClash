using CLHoma.LevelSystem;
using UISystems;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System;
using System.Collections;

namespace CLHoma.Combat
{
    public abstract class BaseEnemyBehavior : MonoBehaviour, IHealth
    {
        #region Constants
        protected const string ANIMATOR_TRIGGER_ATTACK = "IsAttack";
        protected const string ANIMATOR_TRIGGER_DEATH = "IsDead";
        protected const string ANIMATOR_BOOL_RUN = "IsRunning";
        protected const float HEALTH_BAR_OFFSET = 1.5f;
        protected const float ROTATION_SPEED = 5f;
        #endregion

        #region Serialized Fields
        [SerializeField] private EnemyTier tier = EnemyTier.Regular;
        [SerializeField] protected Animator animatorRef;
        [SerializeField] protected HealthbarBehaviour healthbarBehaviour;
        [SerializeField] protected GameObject shawdowObj;
        [SerializeField] protected Renderer renderer;

        [Space]
        [SerializeField] private UIElemental _elementUi;
        [Space]
        [SerializeField] protected float attackAnimationDelay = 0.5f;

        [SerializeField] protected bool isFlying = false;

        protected bool isInAttackAnimation = false;
        protected Coroutine attackCoroutine;
        #endregion

        #region Properties
        public EnemyTier Tier { get => tier; private set => tier = value; }
        protected EnemyStats stats;
        public EnemyStats Stats => stats;
        protected float currentHealth;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => Utils.CalculateEnemyStat((int)stats.Hp, levelEnemy);
        protected bool isDead;
        protected bool isAttacking;
        public bool IsDead => isDead;
        public bool IsAttacking => isAttacking;
        public float AttackCooldown => stats.AttackCooldown;
        public UnityEvent<float> onDamageTaken;
        public UnityEvent<float> onHealthChanged;
        public Action onDeath = null;
        #endregion

        #region Protected Fields
        protected Transform target;
        protected CharacterBehaviour characterBehaviour;
        protected float lastAttackTime;
        protected CapsuleCollider enemyCollider;
        protected Rigidbody enemyRigidbody;
        protected Color originalColor;
        protected int levelEnemy = 1;
        protected ElementType elementType;
        public ElementType ElementType => elementType;
        public int Gold => Utils.CalculateEnemyStat(stats.Gold, levelEnemy, 0.25f);
        public bool IsFlying => isFlying;

        public Transform Target { get => target; set => target = value; }
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            InitializeComponents();
        }

        protected virtual void FixedUpdate()
        {
            //if (GameManager.Instance.IsGamePaused || isDead)
            //    return;

            if (isAttacking)
            {
                //HandleAttack();
                return;
            }

            //MoveTowardsTarget();
        }

        public virtual void OnDestroy()
        {
            if (healthbarBehaviour != null && healthbarBehaviour.HealthBarTransform != null)
            {
                Destroy(healthbarBehaviour.HealthBarTransform.gameObject);
            }
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            isDead = false;
            lastAttackTime = 0f;
            enemyCollider = GetComponent<CapsuleCollider>();
            enemyRigidbody = GetComponent<Rigidbody>();
            originalColor = renderer.material.color;
        }

        public void SetEnemyData(EnemyData enemyData, int level = 1)
        {
            stats = enemyData.Stats;
            levelEnemy = level;
            elementType = enemyData.ElementType;
        }

        public virtual void Initialise()
        {
            //target = PlayerController.Instance.Target;

            currentHealth = MaxHealth;
            isDead = false;
            isAttacking = false;

            InitializeHealthBar();
            InitializeTransform();
            InitializeRigidbody();
            InitializeVisuals();

            if (healthbarBehaviour != null)
                healthbarBehaviour.gameObject.SetActive(false);
        }

        private void InitializeHealthBar()
        {
            if (healthbarBehaviour == null) return;
            healthbarBehaviour.Initialise(transform, this, false, Vector3.zero, 1, tier == EnemyTier.Elite);
        }

        private void InitializeTransform()
        {
            transform.localScale = Vector3.one;
            transform.LookAt(target);
        }

        private void InitializeRigidbody()
        {
            enemyCollider.enabled = true;
            enemyRigidbody.isKinematic = true;
            enemyRigidbody.useGravity = false;
        }

        private void InitializeVisuals()
        {
            shawdowObj.gameObject.SetActive(true);
            renderer.material.color = originalColor;
            animatorRef.transform.localPosition = Vector3.zero;

            if (_elementUi != null)
            {
                _elementUi.Setup(elementType);
            }
            if (healthbarBehaviour != null)
                healthbarBehaviour.DisableBar();
        }
        #endregion

        #region Combat

        public float GetCurrentDamage()
        {
            float baseDamage = Utils.CalculateEnemyStat(stats.Damage.Random(), levelEnemy);
            return baseDamage;
        }

        public void HandleAttack()
        {
            float timeSinceLastAttack = Time.time - lastAttackTime;
            if (timeSinceLastAttack >= AttackCooldown && !isInAttackAnimation)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }

        public virtual void Attack()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }
            attackCoroutine = StartCoroutine(AttackSequence());
        }

        protected virtual IEnumerator AttackSequence()
        {
            isInAttackAnimation = true;
            PlayAttackAnimation();          
            yield return new WaitForSeconds(attackAnimationDelay);          
            PerformAttackAction();
            
            isInAttackAnimation = false;
        }

        protected virtual void PerformAttackAction()
        {

        }

        public virtual void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection, HitType hitType)
        {
            if (damage <= 0 || isDead)
                return;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            if (healthbarBehaviour != null)
                healthbarBehaviour.EnableBar();

            // Notify about damage taken
            onDamageTaken?.Invoke(damage);
            
            // Notify about health change
            onHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                OnDeath();
            }
            else
            {
                if (healthbarBehaviour != null)
                    healthbarBehaviour.OnHealthChanged();
            }
            //ShowDamageText(damage, hitType);

            if (elementType != ElementType.None)
            {
                UIManager.instance.SetTutorialElement();
            }
        }

        private void ShowDamageText(float damage, HitType hitType)
        {
            string textDamage = Mathf.CeilToInt(damage).ToString();
            FloatingTextController.SpawnFloatingText(
                hitType.ToString(),
                textDamage,
                transform.position + Vector3.up * HEALTH_BAR_OFFSET,
                Quaternion.identity,
                .6f
            );
        }

        protected virtual void OnDeath()
        {
            isDead = true;
            // animatorRef.SetTrigger(ANIMATOR_TRIGGER_DEATH);
            animatorRef.SetTrigger("StartDie");
            animatorRef.SetBool("IsDead", true);


            if (healthbarBehaviour != null)
                healthbarBehaviour.DisableBar();
            enemyCollider.enabled = false;
            shawdowObj.gameObject.SetActive(false);
            renderer.material.color = Color.gray;
            onDeath?.Invoke();

            // Spawn resource when enemy dies
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.SpawnResource(transform.position + Vector3.up * 0.25f, Gold);
            }

            // Create a sequence for death animation and delay
            Sequence deathSequence = DOTween.Sequence();
            
            // Add additional delay after animation
            AnimatorStateInfo state = animatorRef.GetCurrentAnimatorStateInfo(0);
            float clipLength = state.length;
            deathSequence.AppendInterval(clipLength+ 0.5f);
            
            // Destroy the object after the sequence completes
            deathSequence.OnComplete(() => {
                WD.GameManager.Instance.UnregisterEnemy(gameObject);
                GameObject.Destroy(gameObject);
            });
        }

        public virtual void Heal(float amount)
        {
            if (IsDead) return;

            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);

            // Notify UI and other listeners about health change
            if (healthbarBehaviour != null)
                healthbarBehaviour.OnHealthChanged();
        }
        #endregion

        #region Movement
        protected void MoveTowardsTarget()
        {
            if (!target)
                return;

            float distanceToTarget = Mathf.Abs(target.position.z - transform.position.z);

            if (distanceToTarget > stats.AttackRange)
            {
                MoveToTarget();
            }
            else if (Time.time >= lastAttackTime + AttackCooldown)
            {
                isAttacking = true;
            }

            UpdateRotation();
        }

        private void MoveToTarget()
        {
            Vector3 direction = new Vector3(0, 0, Mathf.Sign(target.position.z - transform.position.z));
            transform.position += direction * stats.MoveSpeed * Time.fixedDeltaTime;
        }

        private void UpdateRotation()
        {
            Vector3 lookDirection = new Vector3(0, 0, target.position.z - transform.position.z);
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRotation,
                    Time.fixedDeltaTime * ROTATION_SPEED
                );
            }
        }
        #endregion

        #region Animation
        public void ToggleSpeedAnimator(bool value)
        {
            if (animatorRef != null)
            {
                animatorRef.speed = value ? 1 : 0;
            }
        }

        public virtual void PlayRunAnimation(bool isRunning)
        {
            if (animatorRef != null && !isDead && !animatorRef.GetBool("IsMoving"))
            {
                animatorRef.SetFloat("Speed", 1);
                animatorRef.SetTrigger("StartMove");
                animatorRef.SetBool("IsMoving", true);

            }
        }

        public virtual void PlayAttackAnimation()
        {
            if (animatorRef != null && !isDead)
            {
                Debug.Log("Setting attack trigger on animator");
                animatorRef.SetTrigger(ANIMATOR_TRIGGER_ATTACK);
            }
            else
            {
                Debug.LogWarning($"Cannot play attack animation: animator {(animatorRef == null ? "is null" : "exists")}, isDead: {isDead}");
            }
        }
        #endregion

        private void OnEnemyDeath()
        {
            // Implementation of OnEnemyDeath method
        }
    }
}