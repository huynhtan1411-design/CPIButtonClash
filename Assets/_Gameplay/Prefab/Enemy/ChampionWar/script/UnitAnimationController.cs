using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AnimationClipData
{
    public string clipName;
    public AnimationClip animClip;
    public AnimationClipData(string name, AnimationClip clip)
    {
        clipName = name;
        animClip = clip;
    }
}

public class UnitAnimationController : MonoBehaviour
{
    [Header("Animation")]
    public Transform animatorT;
    bool isCastingSkill = false;
    Animator animator;
    public Animator Animator => animator;

    [Space(5)]
    [SerializeField] AnimationClip clipIdle;
    [SerializeField] AnimationClip clipHit;
    [SerializeField] AnimationClip clipDead;

    [SerializeField] AnimationClip clipAttack;
    [SerializeField] AnimationClip clipAttack2;
    [SerializeField] AnimationClip clipCrit;
    
    [SerializeField] float animationAttackDelay = 0;

    [Space(5)]
    [SerializeField] AnimationClip clipRun;
    [SerializeField] AnimationClip clipWalk;

    [Space(5)]
    [SerializeField] AnimationClip clipChargeSkill;
    [SerializeField] AnimationClip clipSkill;
    //public AnimationClip clipCastEnd;

    [SerializeField] AnimationClip clipVictory;

    [SerializeField] AnimationClip clipResurrect;

    [SerializeField] AnimationClip clipStun;

    public float ClipAttackLength { get; private set; }

    private int idxAtk;

    private void Awake()
    {
        InitAnimation();
    }

    public bool CheckCurrentClipFinish(string Name)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(Name))
        {
            if (stateInfo.normalizedTime >= 0.75f)
                return true;
        }
        return false;
    }

    public bool CheckCurrentClipName(string Name)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(Name))
        {
                return true;
        }
        return false;
    }
    public float GetCurrentAnimationLength() {
        if (Animator?.runtimeAnimatorController == null)
            return 0f;

        var clips = Animator.GetCurrentAnimatorClipInfo(0);
        return (clips.Length > 0 && clips[0].clip != null) ? clips[0].clip.length : 0f;
    }

    float cachSpeed = 0;
// #if UNITY_EDITOR
//    private void Update()
//    {
//        if (CheckCurrentClipFinish("Hit"))
//        {
//            animator.SetBool("IsHit", false);

//            if (cachSpeed > 0)
//                PlayRun();
//            else
//                PlayIdle();
//        }
//        //test
//        if (Input.GetKeyDown(KeyCode.Alpha1))
//        {
//            PlayIdle();
//        }

//        if (Input.GetKeyDown(KeyCode.Alpha2))
//        {
//            PlayRun();
//        }

//        if (Input.GetKeyDown(KeyCode.Alpha3))
//        {
//            PlayAttack();
//        }
//    }
// #endif

    public bool IsPlayingAttack()
    {
        return animator.GetBool("Attack");
    }
    public bool IsHit()
    {
        if(animator)
            return animator.GetBool("IsHit");
        return false;
    }

    public bool IsPlayingIdle()
    {
        return animator.GetBool("IsIdle");
    }
    
    public bool IsPlayingUltimate()
    {
        return animator.GetBool("IsCastingSkill");
    }

    public bool IsPlayingMove() {
        return animator.GetBool("IsRun") || animator.GetBool("IsWalk");
    }
    
    public bool IsPlayingStun()
    {
        return animator.GetBool("IsStun");
    }

    public float GetAttackIndex() {
        return animator.GetFloat("AttackIndex");
    }

    public void InitAnimation()
    {
        if (animatorT != null)
            animator = animatorT.GetComponent<Animator>();
        if (animator == null)
            return;
        RuntimeAnimatorController standardController = Resources.Load<RuntimeAnimatorController>($"Controller/StandardController");
        //Replace some zombie animator with game animator 
        if (animator.runtimeAnimatorController != standardController)
        {
            // Replace the current controller with StandardController
            animator.runtimeAnimatorController = standardController;
            Debug.Log("Animator controller replaced with StandardController.");
        }
        //AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        if (animator.runtimeAnimatorController is AnimatorOverrideController == false)
        {
            AnimatorOverrideController aniOverrideController = new AnimatorOverrideController();
            aniOverrideController.name = (this.gameObject.name + "animator");
            aniOverrideController.runtimeAnimatorController = animator.runtimeAnimatorController;
            animator.runtimeAnimatorController = aniOverrideController;
            
            if (clipIdle != null) aniOverrideController["DummyIdle"] = clipIdle;
            if (clipHit != null) aniOverrideController["DummyHit"] = clipHit;
            
            if (clipAttack != null)
            {
                aniOverrideController["DummyAttack"] = clipAttack;
                ClipAttackLength = clipAttack.length;
            }
            if (clipAttack2 != null) aniOverrideController["DummyAttack_2"] = clipAttack2;
            if (clipCrit != null) aniOverrideController["DummyCrit"] = clipCrit;

            if(clipChargeSkill != null) aniOverrideController["DummyChargeSkill"] = clipChargeSkill;
			if (clipSkill != null) aniOverrideController["DummyCast"] = clipSkill;
            
            if (clipResurrect != null) aniOverrideController["DummyResurrect"] = clipResurrect;

            if (clipDead != null) aniOverrideController["DummyDestroyed"] = clipDead;
            if (clipVictory != null) aniOverrideController["DummyFinish"] = clipVictory;


            if (clipRun != null) aniOverrideController["DummyMove"] = clipRun;
            if (clipWalk != null) aniOverrideController["DummyWalk"] = clipWalk;
            
            if (clipStun != null) aniOverrideController["DummyStun"] = clipStun;

        }
    }

    public void PlayIdle()
    {
        if (animator != null && clipIdle != null)
        {
            animator.SetTrigger("StartIdle");
            animator.SetBool("IsIdle", true);
            animator.SetFloat("Speed", 0);
            if (clipDead != null)
                animator.SetBool("Destroyed", false);
            isCastingSkill = false;
            cachSpeed = 0;
        }

    }


    public float PlayResurrect()
    {
        if (animator != null && clipResurrect != null)
        {
            animator.SetTrigger("StartResurrect");
            animator.SetBool("IsResurrect", true);
            if (clipDead != null)
                animator.SetBool("Destroyed", false);
            isCastingSkill = false;
            return clipResurrect.length;
        }
        return 0f;
    }

    public void PlayWalk()
    {
        if (animator != null) {
            animator.SetFloat("Speed", 1);
            animator.SetBool("IsWalk", true);
        }
    }

    public void PlayRun()
    {
        if (animator != null) {
            // animator.SetFloat("Speed", Unit.UnitStat.moveSpeed);
            animator.SetFloat("Speed", 1);
            animator.SetBool("IsWalk", false);
            animator.SetBool("IsRun", true);
        }
        // cachSpeed = Unit.UnitStat.moveSpeed;
    }

    public void PlayHit()
    {
        if (animator != null && clipHit != null) {
            animator.SetTrigger("Hit");
            animator.SetFloat("Speed", 0);
            animator.SetBool("IsHit", true);
        }

    }

    public float PlayDead()
    {
        if (animator == null)
            return 0;
        if (clipDead != null)
            animator.SetBool("Destroyed", true);
        return clipDead != null ? clipDead.length : 0;
    }

    public float PlayAttack(bool isCrit = false)
    {
        // return PlayAttack(Unit?.GetAtkSpd() ?? 100f, isCrit);
        return 0;
    }

    public float PlayAttack(float atkSpeed, bool isCrit = false)
    {
        if (animator == null)
            return 0;

        float atkSpeedAnim = Mathf.Max(0f, atkSpeed / 100f);

        animator.SetFloat("AttackSpeedBonus", atkSpeedAnim);
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsRun", false);
        animator.SetBool("IsIdle", false);
        animator.SetFloat("Speed", 0);
        animator.ResetTrigger("StartIdle");

        cachSpeed = 0;

        if (isCrit)
        {
            animator.SetFloat("AttackIndex", 2);
        }
        else
        {
            animator.SetFloat("AttackIndex", clipAttack2 != null ? idxAtk : 0);
            idxAtk = 1 - idxAtk; // Toggle between 0 and 1
        }

        if (clipAttack != null)
            animator.SetTrigger("Attack");

        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animator.Play(stateInfo.fullPathHash, 0, 0f);
        
        return animationAttackDelay;
    }

	public void PlayVictory()
    {
        if (animator == null)
            return;

        if (clipVictory != null)
            animator.SetTrigger("StartFinish");
        // castingSkillDuraiton += clipFinish.length;
    }

    public void PlayChargeUltimate(bool resetStateInfo = false, float castingSkillDuration = -1) {
		if (animator == null)
			return;

        if(clipChargeSkill != null) {
			animator.SetBool("IsIdle", false);
            animator.SetTrigger("ChargeSkill");
			animator.SetBool("IsCastingSkill", true);
		}

        if (resetStateInfo) {
			var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			animator.Play(stateInfo.fullPathHash, 0, 0f);
		}
	}

    public void PlayUltimate(bool resetStateInfo = false, float castingSkillDuration = -1)
    {
        if (animator == null)
            return;

        if (clipSkill != null)
        {
            animator.SetBool("IsIdle", false);
            animator.SetTrigger("CastDone");
            animator.SetBool("IsCastingSkill", true);
            animator.SetFloat("CastIndex", 0);
        }

        isCastingSkill = true;

        if (resetStateInfo)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, 0, 0f);
        }
    }

    public void PlayStun()
    {
        if (animator == null)
            return;
        if (isCastingSkill)
            animator.ResetTrigger("Attack");

        if (clipStun != null)
        {
            animator.SetTrigger("StartStun");
            animator.SetBool("IsStun", true);
        }
        animator.SetFloat("Speed", 0);

        isCastingSkill = false;
    }

    public void StopStun()
    {
        if (animator == null)
            return;

        if (clipStun != null) animator.SetBool("IsStun", false);
        animator.SetFloat("Speed", 0);
        isCastingSkill = false;
    }

    public void StopUltimate()
    {
        if (animator == null)
            return;
        isCastingSkill = false;
        if (clipSkill != null)
            animator.SetBool("IsCastingSkill", false);
    }

    public virtual void OnDrawGizmos()
    {
        // if (Unit == null)
        //     return;

        //Gizmos.color = Color.red;
        //if (Unit.HasTarget()) {
        //    for (int i = 0; i < Unit.targetingList.Count; i++) {
        //        Debug.DrawLine(Unit.GetPos(), Unit.targetingList[i].GetPos());
        //    }
        //}

        /*
        if(UseDirectionalTargeting()){
            Vector3 v1=thisT.rotation*Quaternion.Euler(0, targetingDir+targetingFov/2, 0)*new Vector3(0, 0, 3);
            Vector3 v2=thisT.rotation*Quaternion.Euler(0, targetingDir-targetingFov/2, 0)*new Vector3(0, 0, 3);
            Debug.DrawLine(GetPos(), GetPos()+v1);
            Debug.DrawLine(GetPos(), GetPos()+v2);
        }
        */
    }
}