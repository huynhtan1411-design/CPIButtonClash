using TemplateSystems.Keys;
using UnityEngine;

namespace TemplateSystems.Controllers.Player
{
    public enum PlayerAnimationStates
    {
        Idle,
        Run,
        Attack,
        HorseIdle,       // Player sitting on idle horse
        HorseRide,       // Player riding moving horse
        HorseAttack,     // Player attacking while on horse
        Death
    }

    public class PlayerAnimationController : CharacterAnimationController<PlayerAnimationStates>
    {
        #region Self Variables
        public bool InBase = false;
        public bool IsOnHorse { get; private set; }

        #region Serialized Variables
        [SerializeField] private MeshCollider meshColliderAxe;
        [SerializeField] private Animator horseAnimator;  // Reference to horse's animator
        #endregion

        #region Animation Parameters
        private const string HORSE_MOVE_PARAM = "IsMoving";
        private const string ON_HORSE_PARAM = "IsOnHorse";
        private const string ATTACK_TRIGGER = "Shoot";
        #endregion

        #endregion

        private void Start()
        {
            SubscribeEvents();
            IsOnHorse = false;
            MountHorse(horseAnimator);
        }

        private void SubscribeEvents()
        {
            InputSignals.Instance.onJoystickDragged += OnJoystickDragged;
            InputSignals.Instance.onInputTaken += OnPointerDown;
            InputSignals.Instance.onInputReleased += OnInputReleased;
        }

        private void UnsubscribeEvents()
        {
            if (InputSignals.Instance == null)
                return;
            InputSignals.Instance.onInputTaken -= OnPointerDown;
            InputSignals.Instance.onInputReleased -= OnInputReleased;
            InputSignals.Instance.onJoystickDragged -= OnJoystickDragged;
        }

        private void OnJoystickDragged(IdleInputParams arg0)
        {
            
            if (IsOnHorse)
            {
                ChangeAnimationState(PlayerAnimationStates.HorseRide);
                if (horseAnimator != null)
                {
                    horseAnimator.SetBool(HORSE_MOVE_PARAM, true);
                }
            }
        }

        public void OnPointerDown()
        {
            animator.SetBool("IsMoving", true);
            animator.SetTrigger("StartMove");
            if (IsOnHorse)
            {
                ChangeAnimationState(PlayerAnimationStates.HorseRide);
                if (horseAnimator != null)
                {
                    horseAnimator.SetBool(HORSE_MOVE_PARAM, true);
                }
                
            }
            else
            {
                ChangeAnimationState(PlayerAnimationStates.Run);
            }
        }

        public void OnInputReleased()
        {
            animator.SetBool("IsMoving", false);
            if (IsOnHorse)
            {
                ChangeAnimationState(PlayerAnimationStates.HorseIdle);
                if (horseAnimator != null)
                {
                    horseAnimator.SetBool(HORSE_MOVE_PARAM, false);
                }
            }
            else
            {
                ChangeAnimationState(PlayerAnimationStates.Idle);
            }
        }

        public override void ChangeAnimationState(PlayerAnimationStates animationStates)
        {
            if (animator == null) return;

            if (animationStates == PlayerAnimationStates.Attack || animationStates == PlayerAnimationStates.HorseAttack)
            {
                // animator.SetTrigger(ATTACK_TRIGGER);
                // animator.SetTrigger("StartAttack");
                Debug.Log("Play Attack Animation");
                PlayAttack(100, false);
                return;
            }

            animator.Play(animationStates.ToString());
        }
        private int idxAtk = 1;
        public void PlayAttack(float atkSpeed, bool isCrit = false)
        {
            if (animator == null)
                return ;
            animator.SetBool("IsWalk", false);
            animator.SetBool("IsRun", false);
            animator.SetBool("IsIdle", false);
            animator.SetFloat("Speed", 0);
            // animator.ResetTrigger("StartIdle");


            if (isCrit)
            {
                animator.SetFloat("AttackIndex", 0);
            }
            else
            {
                animator.SetFloat("AttackIndex", idxAtk);
                idxAtk++; // Toggle between 0 and 1
            }

            animator.SetTrigger("StartAttack");
            if(idxAtk == 3)
            {
                idxAtk = 1;
            }
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, 0, 0f);
            
        }

        public void MountHorse(Animator horseAnim)
        {
            IsOnHorse = true;
            horseAnimator = horseAnim;
            if (animator != null)
            {
                animator.SetBool(ON_HORSE_PARAM, true);
            }
            ChangeAnimationState(PlayerAnimationStates.HorseIdle);
        }

        public void DismountHorse()
        {
            IsOnHorse = false;
            if (animator != null)
            {
                animator.SetBool(ON_HORSE_PARAM, false);
            }
            if (horseAnimator != null)
            {
                horseAnimator.SetBool(HORSE_MOVE_PARAM, false);
            }
            horseAnimator = null;
            ChangeAnimationState(PlayerAnimationStates.Idle);
        }

        public void PlayAttackAnimation()
        {
            if (IsOnHorse)
            {
                ChangeAnimationState(PlayerAnimationStates.HorseAttack);
            }
            else
            {
                ChangeAnimationState(PlayerAnimationStates.Attack);
            }
        }

        public void DisableColliderAxe()
        {
            meshColliderAxe.enabled = false;
        }

        public void EnableColliderAxe()
        {
            meshColliderAxe.enabled = true;
        }
    }
}