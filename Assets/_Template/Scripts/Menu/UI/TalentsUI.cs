using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TemplateSystems;
using Newtonsoft.Json;
using UISystems;

public class TalentsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TalentCardUI[] talentCards; //Attack, HP, Defense
    [SerializeField] private LockNotiUI lockNotiUI;
    [SerializeField] private Button prevHeroButton;
    [SerializeField] private Button nextHeroButton;
    [SerializeField] private ParticleSystem upgradeEffect;

    private int currentHeroIndex = 0;
    private string currentHeroId;
    private EquipmentHeroData equipmentHeroDataCur;

    private void OnEnable()
    {
        Setup("0");
    }

    public void Setup(string heroId)
    {
        currentHeroId = heroId;
        UpdateHeroDisplay();
        UpdateTalentCards();

        if (DataManager.COINS >= 100)
            UIManager.instance.SetTutorial(TutorialStepID.Learn_Talent);
        else
        {
            TutorialManager.SaveTutorial(8);
        }
    }

    private void UpdateHeroDisplay()
    {
        if (DataManager.Instance.HeroEquipmentData.ContainsKey(currentHeroId))
            equipmentHeroDataCur = DataManager.Instance.HeroEquipmentData[currentHeroId];
        else
        {
            equipmentHeroDataCur = new EquipmentHeroData { Talents = new TalentData() }; // Khởi tạo TalentData nếu chưa có
            DataManager.Instance.HeroEquipmentData[currentHeroId] = equipmentHeroDataCur;
        }

        HerosInfoData hero = DataManager.Instance.GetInfoDataHero(currentHeroId);
        heroNameText.text = hero.Name;
        coinText.text = DataManager.COINS.ToString();
        //var (totalHP, totalAttack, totalDefense) = CalculateTotalStats();
        //hpText.text = "HP: " + totalHP;
        //attackText.text = "Attack: " + totalAttack;
        //defenseText.text = "Attack: " + totalAttack;
        Hero3dUI.Instance.ShowHeroById(currentHeroId);
        prevHeroButton.onClick.RemoveAllListeners();
        prevHeroButton.onClick.AddListener(NextHero);
        nextHeroButton.onClick.RemoveAllListeners();
        nextHeroButton.onClick.AddListener(PrevHero);
        CheckShowLockHeros(currentHeroId);
    }

    private (int totalHP, int totalAttack, int totalDefense) CalculateTotalStats()
    {
        int totalHP = 0;
        int totalAttack = 0;
        int totalDefense = 0;
        var heroInfo = DataManager.Instance.GetInfoDataHero(currentHeroId);
        if (heroInfo != null)
        {
            totalHP = heroInfo.HP;
            totalAttack = heroInfo.Damage;
        }

        //  bonus Talents
        var talents = equipmentHeroDataCur.Talents;
        totalHP += GetTalentBonus(talents.LevelHP, "HP");
        totalAttack += GetTalentBonus(talents.LevelAttack, "Attack");
        totalDefense += GetTalentBonus(talents.LevelAttack, "Defense");

        foreach (var equipment in equipmentHeroDataCur.Equipment.Values)
        {
            if (equipment != null)
            {
                totalHP += equipment.HP;
                totalAttack += equipment.Damage;
            }
        }

        return (totalHP, totalAttack, totalDefense);
    }

    public int GetTalentBonus(int level, string type)
    {
        int totalBonus = 0;
        foreach (var data in DataManager.Instance.TalentsInfoData.Data)
        {
            if (data.Level <= level) 
            {
                switch (type)
                {
                    case "HP":
                        totalBonus += data.HP;
                        break;
                    case "Attack":
                        totalBonus += data.Attack;
                        break;
                    case "Defense":
                        totalBonus += data.Defense;
                        break;
                }
            }
        }
        return totalBonus;
    }

    private void UpdateTalentCards()
    {
        var talents = equipmentHeroDataCur.Talents;
        talentCards[0].Setup("Attack", talents.LevelAttack, OnUpgradeTalent);
        talentCards[1].Setup("HP", talents.LevelHP, OnUpgradeTalent);
        //talentCards[2].Setup("Defense", talents.LevelDefense, OnUpgradeTalent);
    }

    private void OnUpgradeTalent(string talentType)
    {
        UIManager.instance.CloseTutorial(TutorialStepID.Learn_Talent);
        var talents = equipmentHeroDataCur.Talents;
        int currentLevel = 0;
        switch (talentType)
        {
            case "Attack": currentLevel = talents.LevelAttack; break;
            case "HP": currentLevel = talents.LevelHP; break;
            case "Defense": currentLevel = talents.LevelDefense; break;
        }

        int nextLevel = currentLevel + 1;
        int price = GetTalentPrice(nextLevel);
        if (DataManager.COINS >= price)
        {
            DataManager.COINS -= price;
            PlayerPrefsManager.SaveCoins(DataManager.COINS);

            switch (talentType)
            {
                case "Attack": talents.LevelAttack = nextLevel; break;
                case "HP": talents.LevelHP = nextLevel; break;
                case "Defense": talents.LevelDefense = nextLevel; break;
            }

            DataManager.Instance.SaveData();
            UpdateHeroDisplay();
            UpdateTalentCards();
            UIManager.instance.UpdateCoins();
            upgradeEffect.Play();
        }
        else
        {
            Debug.LogWarning("Not enough coins to upgrade talent: " + talentType);
        }
    }

    private int GetTalentPrice(int level)
    {
        foreach (var data in DataManager.Instance.TalentsInfoData.Data)
        {
            if (data.Level == level)
                return data.Gold;
        }
        return int.MaxValue; 
    }

    public bool CanUpgradeTalent()
    {
        var talents = equipmentHeroDataCur.Talents;
        int attackPrice = GetTalentPrice(talents.LevelAttack + 1);
        int hpPrice = GetTalentPrice(talents.LevelHP + 1);
        
        return DataManager.COINS >= attackPrice || DataManager.COINS >= hpPrice;
    }

    private void CheckShowLockHeros(string id)
    {
        HerosInfoData hero = DataManager.Instance.GetInfoDataHero(id);
        int level = DataManager.Instance.GetLevel();
        if (hero.LevelUnlock <= level)
            lockNotiUI.Hide();
        else
            lockNotiUI.Show("Unlock at chapter " + (hero.LevelUnlock + 1));
    }
    public void PrevHero()
    {
        var heroes = DataManager.Instance.HerosInfoData.Data;
        if (heroes.Count == 0) return;
        currentHeroIndex = (currentHeroIndex - 1 + heroes.Count) % heroes.Count;
        currentHeroId = heroes[currentHeroIndex].Id;
        Setup(currentHeroId);
    }

    public void NextHero()
    {
        var heroes = DataManager.Instance.HerosInfoData.Data;
        if (heroes.Count == 0) return;
        currentHeroIndex = (currentHeroIndex + 1) % heroes.Count;
        currentHeroId = heroes[currentHeroIndex].Id;
        Setup(currentHeroId);
    }
}
