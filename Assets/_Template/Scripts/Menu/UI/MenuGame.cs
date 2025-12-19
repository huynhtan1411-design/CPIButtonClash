using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UISystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CLHoma;

public class MenuGame : MonoBehaviour
{
    [SerializeField] private SkillCardUI[] skillCardUIs;
    [SerializeField] private SkillTypeColors skillTypeColors;
    [SerializeField] private Image[] imageIconHeros;
    [SerializeField] private Image[] imageIconSkills;
    private Dictionary<string, int> skillCardCounts = new Dictionary<string, int>();
    private List<string> selectedSkills = new List<string>();
    public List<string> SelectedSkills { get => selectedSkills; set => selectedSkills = value; }
    private Dictionary<SkillType, Action<LevelConfig.SkillCardData>> skillActions = new Dictionary<SkillType, Action<LevelConfig.SkillCardData>>();

    public SkillTypeColors SkillTypeColors { get => skillTypeColors; set => skillTypeColors = value; }

    private void Start()
    {
        UIManager.onMenuGameSet += InitUI;
    }

    private void OnDestroy()
    {
        UIManager.onMenuGameSet -= InitUI;
    }

    private void InitUI()
    {
        UpdateSkillCardsUI();
        //UpdateSelectedHeroIcons();
        UpdateSelectedSkillIcons();
    }


    public void SetAction(SkillType type, Action<LevelConfig.SkillCardData> action)
    {
        skillActions[type] = action;
    }

    public void ExecuteSkillAction(LevelConfig.SkillCardData data)
    {
        if(SkillManager.Instance != null)
          SkillManager.Instance.LearnSkill(data.cardId);
        //IncrementSkillCardCount(data.cardId);
        if (!selectedSkills.Contains(data.cardId))
            selectedSkills.Add(data.cardId);
        //skillActions[data.skillType](data);
        UIManager.instance.SetResume();
        Vibration_Manager.instance.Vibrate(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
    }

    public void UpdateSkillCardsUI()
    {
        var availableCards = SkillManager.Instance.GetAvailableSkillCards();
        for (int i = 0; i < skillCardUIs.Length; i++)
        {
            if (i < availableCards.Count)
            {
                LevelConfig.SkillCardData data = availableCards[i];
                skillCardUIs[i].Setup(availableCards[i], data.starCount, this);
            }
            else
            {
                skillCardUIs[i].gameObject.SetActive(false);
            }
        }

    }
    private void UpdateSelectedHeroIcons()
    {
        var lst = SkillManager.Instance.GetListSkillWithType(AbilityType.Hero);
        for (int i = 0; i < imageIconHeros.Length; i++)
        {
            if (i < lst.Count)
            {
                string heroId = lst[i].skillData.Id;
                Image iconImage = imageIconHeros[i];
                LoadIcon(heroId, iconImage);
            }
            else
            {
                imageIconHeros[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSelectedSkillIcons()
    {
        var lst = SkillManager.Instance.GetListSkillWithType(AbilityType.Special);
        for (int i = 0; i < imageIconSkills.Length; i++)
        {
            if (i < lst.Count)
            {
                string skillId = lst[i].skillData.Id;
                Image iconImage = imageIconSkills[i];
                LoadIcon(skillId, iconImage);
            }
            else
            {
                imageIconSkills[i].gameObject.SetActive(false);
            }
        }
    }

    private LevelConfig.SkillCardData FindSkillCardData(string cardId)
    {
        return GameManagerTest.Instance.LevelConfig.skillCards.Find(data => data.cardId == cardId);
    }
    public async void LoadIcon(string id, Image image)
    {
        string iconAddress = "icons/" + id + ".png";
        var handle = Addressables.LoadAssetAsync<Sprite>(iconAddress);
        await handle.Task;

        if (handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded)
        {
            image.sprite = handle.Result;
            image.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Failed to load icon at " + iconAddress);
            image.gameObject.SetActive(false);
        }
    }

    public int GetSkillCardCount(string cardId)
    {
        if (skillCardCounts.ContainsKey(cardId))
        {
            return skillCardCounts[cardId];
        }
        return 0;
    }

    private void IncrementSkillCardCount(string cardId)
    {
        if (skillCardCounts.ContainsKey(cardId))
        {
            skillCardCounts[cardId]++;
        }
        else
        {
            skillCardCounts[cardId] = 1;
        }
    }
}