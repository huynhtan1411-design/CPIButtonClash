using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLHoma.Combat;
using Unity.Burst.CompilerServices;
using TemplateSystems;
using System;
using UnityEngine.UI;
using UISystems;
public class SlotBehavior : MonoBehaviour
{
    public enum SlotState
    {
        None,
        Red,
        Green,
    }
    [SerializeField] GameObject highlightObj;
    [SerializeField] Image plusImage;
    [SerializeField] bool IsTutorialSlot = false;
    private CharacterBehaviour characterBehaviour;
    public CharacterBehaviour CharacterBehaviour => characterBehaviour;

    public void SetCharacterBehaviour(CharacterBehaviour character)
    {
        characterBehaviour = character;
    }
    public void SetHighlight(SlotState state)
    {
        if (state == SlotState.None)
        {
            highlightObj.SetActive(false);
        }
        else if (state == SlotState.Red)
        {
            highlightObj.SetActive(true);
            highlightObj.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else if (state == SlotState.Green)
        {
            highlightObj.SetActive(true);
            highlightObj.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }

    public void HandleCharacter(HeroCardData cardData, Action success, Action fail)
    {
        if (characterBehaviour != null && characterBehaviour.CanUpgradeWithCard(cardData))
        {
            characterBehaviour.UpgradeHero(cardData);
            success?.Invoke();
        }
        else
        {
            fail?.Invoke();
        }
    }

    public bool IsEmpty()
    {
        return transform.GetChild(0).childCount == 0;
    }

    public void ToggleRecommend(bool value, Color color)
    {
        plusImage.gameObject.SetActive(value);
        plusImage.color = color;
    }

    public bool IsTutorialSummonSlotActive()
    {
        return !IsTutorialSlot && TutorialManager.GetTutorial(0) == 0;
    }
    public bool IsTutorialMergeSlotActive()
    {
        return !IsTutorialSlot && TutorialManager.GetTutorial(1) == 0;
    }
}