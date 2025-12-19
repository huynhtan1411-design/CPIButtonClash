using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterAnimationController<T> : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
        }
    }

    public abstract void ChangeAnimationState(T animationState);
}
