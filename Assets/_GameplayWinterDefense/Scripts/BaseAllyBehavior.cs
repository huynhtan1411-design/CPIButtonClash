using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using CLHoma.Combat;
using DG.Tweening;
using WD;

namespace WD
{
    public class BaseAllyBehavior : MonoBehaviour, IHealth
    {
        #region Constants
        protected const string ANIMATOR_TRIGGER_ATTACK = "Shoot";
        protected const string ANIMATOR_TRIGGER_DEATH = "IsDead";
        protected const string ANIMATOR_BOOL_RUN = "IsRunning";
        protected const float HEALTH_BAR_OFFSET = 1.5f;
        protected const float ROTATION_SPEED = 5f;
        #endregion

        #region Serialized Fields
        [SerializeField] protected Animator animatorRef;
        [SerializeField] protected GameObject shadowObj;
        [SerializeField] protected Renderer renderer;
        [SerializeField] protected float attackAnimationDelay = 0.5f;
        
        protected bool isInAttackAnimation = false;
        protected Coroutine attackCoroutine;
        #endregion

        #region Properties
        protected AllyStats stats;
        public AllyStats Stats => stats;
        protected float currentHealth;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => stats.Health.firstValue;
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
        protected float lastAttackTime;
        protected CapsuleCollider allyCollider;
        protected Rigidbody allyRigidbody;
        protected Color originalColor;

        public Transform Target { get => target; set => target = value; }
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            InitializeComponents();
        }

        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            isDead = false;
            lastAttackTime = 0f;
            allyCollider = GetComponent<CapsuleCollider>();
            allyRigidbody = GetComponent<Rigidbody>();
            originalColor = renderer.material.color;
        }

        public void SetAllyData(AllyStats stats)
        {
            this.stats = stats;
        }

        public virtual void Initialize()
        {
            currentHealth = MaxHealth;
            isDead = false;
            isAttacking = false;

            InitializeHealthBar();
            InitializeTransform();
            InitializeRigidbody();
            InitializeVisuals();
        }

        private void InitializeHealthBar()
        {
        }

        private void InitializeTransform()
        {
            transform.localScale = Vector3.one;
            if (target != null)
            {
                transform.LookAt(target);
            }
        }

        private void InitializeRigidbody()
        {
            allyCollider.isTrigger = true;
            allyRigidbody.isKinematic = true;
            allyRigidbody.useGravity = false;
        }

        private void InitializeVisuals()
        {
            shadowObj.gameObject.SetActive(true);
            renderer.material.color = originalColor;
            animatorRef.transform.localPosition = Vector3.zero;
        }
        #endregion

        #region Combat
        public float GetCurrentDamage()
        {
            return stats.Damage.firstValue;
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
            // Get the AllyAI component and delegate attack to it
            AllyAI allyAI = GetComponent<AllyAI>();
            if (allyAI != null)
            {
                allyAI.PerformAttack();
            }
        }

        public virtual void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection, HitType hitType)
        {
            if (damage <= 0 || isDead)
                return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            onDamageTaken?.Invoke(damage);
            onHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                OnDeath();
            }
        }

        public virtual void OnDeath()
        {
            isDead = true;
            animatorRef.SetTrigger(ANIMATOR_TRIGGER_DEATH);
            allyCollider.isTrigger = false;
            shadowObj.gameObject.SetActive(false);
            renderer.material.color = Color.gray;
            onDeath?.Invoke();
            
            Sequence deathSequence = DOTween.Sequence();
            
            deathSequence.AppendInterval(1f);
            
            deathSequence.AppendInterval(0.5f);
            
            deathSequence.OnComplete(() => {
                GameObject.Destroy(gameObject);
            });
        }


        public virtual void PlayRunAnimation(bool isRunning)
        {
            if (animatorRef != null && !isDead)
            {
                animatorRef.SetBool(ANIMATOR_BOOL_RUN, isRunning);
            }
        }

        public virtual void PlayAttackAnimation()
        {
            if (animatorRef != null && !isDead)
            {
                animatorRef.SetTrigger(ANIMATOR_TRIGGER_ATTACK);
            }
        }

        public virtual void StopAttackAnimation()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            isInAttackAnimation = false;
            isAttacking = false;
        }
        #endregion
    }
} 