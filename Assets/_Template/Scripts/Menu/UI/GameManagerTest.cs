using UnityEngine;
using System.Collections.Generic;
using UISystems;
using System;
using TemplateSystems;

public class GameManagerTest : MonoBehaviour
{
   public static GameManagerTest Instance { get; private set; }
    public LevelConfig LevelConfig { get => levelConfig; set => levelConfig = value; }
    private Dictionary<string, Dictionary<string, string>> heroEquipment = new Dictionary<string, Dictionary<string, string>>();
    private List<EquipmentItemData> availableEquipment = new List<EquipmentItemData>();

    //public List<string> SelectedHeroes => selectedHeroes;
    [SerializeField] private LevelConfig levelConfig;
    public event Action OnLevelChanged;
    private UIGameCtr uIGameplayCtr;
    private MenuGame menuGame;
    private UIMenuPause uIMenuPause;
    private void Awake()
    {
        Instance = this;
        menuGame = UIManager.instance.MenuGameCtr;
        uIGameplayCtr = UIManager.instance.UIGameplayCtr;
        uIMenuPause = UIManager.instance.UIPauseCtr;
        UIManager.onGameSet += InitUIGame;
        UIManager.onMenuPauseSet += InitUIPause;

        if (Instance == null) Instance = this;
    }

    private void OnDestroy()
    {
        UIManager.onGameSet -= InitUIGame;
        UIManager.onMenuPauseSet -= InitUIPause;
    }

    private void Start()
    {

        if (menuGame != null)
        {
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S))
        {
            UIManager.instance.SetMenuGame();
        }
#endif
    }

    void InitUIGame()
    {
        //uIGameplayCtr.ListViewSkills.UpdateList(idItems);
    }

    void InitUIPause()
    {
        Debug.LogError("InitUIPause");
        List<string> selectedHeroes = new List<string>();
        List<string> selectedSkills = new List<string>();

        foreach (var hero in CLHoma.SkillManager.Instance.GetListSkillWithType(CLHoma.AbilityType.Hero))
        {
            selectedHeroes.Add(hero.skillData.Id);
        }

        foreach (var skill in CLHoma.SkillManager.Instance.GetListSkillWithType(CLHoma.AbilityType.Passive))
        {
            selectedSkills.Add(skill.skillData.Id);
        }

        uIMenuPause.ShowUI(selectedHeroes, selectedSkills);
    }

    public Dictionary<string, string> GetHeroEquipment(string heroId)
    {
        if (!heroEquipment.ContainsKey(heroId))
        {
            heroEquipment[heroId] = new Dictionary<string, string>
            {
                { "Weapons", "" }, { "Rings", "" }, { "Necklaces", "" },
                { "Hat", "" }, { "Shirt", "" }, { "Pants", "" }
            };
        }
        return heroEquipment[heroId];
    }

    public void EquipItem(string heroId, EquipmentItemData equipItemData)
    {
        //var equipment = GetHeroEquipment(heroId);
        //TypeEquipment typeEquipment = equipItemData
        //equipment[slot.ToString()] = itemId;
        //availableEquipment.RemoveAll(e => e.Id == itemId);
    }

    public void UnequipItem(string heroId, string itemId)
    {
        //var equipment = GetHeroEquipment(heroId);
        //foreach (var slot in equipment)
        //{
        //    if (slot.Value == itemId)
        //    {
        //        equipment[slot.Key] = "";
        //        availableEquipment.Add(new EquipmentItemData { Id = itemId, Level = 1 });
        //        break;
        //    }
        //}
    }

    public List<EquipmentItemData> GetAvailableEquipment()
    {
        return availableEquipment;
    }

    //public void SaveHeroEquipment()
    //{
    //    // Lưu danh sách heroIds
    //    string heroIdsStr = string.Join(",", selectedHeroes);
    //    PlayerPrefs.SetString("heroIds", heroIdsStr);

    //    // Lưu từng equipment của hero
    //    foreach (var heroId in selectedHeroes)
    //    {
    //        var equipment = GetHeroEquipment(heroId);
    //        string json = JsonUtility.ToJson(new EquipmentSlots { slots = equipment });
    //        PlayerPrefs.SetString("heroEquipment_" + heroId, json);
    //    }
    //}

    //public void LoadHeroEquipment()
    //{
    //    // Tải danh sách heroIds
    //    string heroIdsStr = PlayerPrefs.GetString("heroIds", "");
    //    if (!string.IsNullOrEmpty(heroIdsStr))
    //    {
    //        selectedHeroes = new List<string>(heroIdsStr.Split(','));
    //    }
    //    else
    //    {
    //        selectedHeroes = new List<string>();
    //    }

    //    // Tải từng equipment của hero
    //    heroEquipment.Clear();
    //    foreach (var heroId in selectedHeroes)
    //    {
    //        string json = PlayerPrefs.GetString("heroEquipment_" + heroId, "");
    //        if (!string.IsNullOrEmpty(json))
    //        {
    //            var equipmentSlots = JsonUtility.FromJson<EquipmentSlots>(json);
    //            heroEquipment[heroId] = equipmentSlots.slots;
    //        }
    //        else
    //        {
    //            heroEquipment[heroId] = new Dictionary<string, string>
    //            {
    //                {"Weapons", ""}, {"Rings", ""}, {"Necklaces", ""},
    //                {"Hat", ""}, {"Shirt", ""}, {"Pants", ""}
    //            };
    //        }
    //    }
    //}

    //public Dictionary<string, string> GetHeroEquipment(string heroId)
    //{
    //    if (!heroEquipment.ContainsKey(heroId))
    //    {
    //        heroEquipment[heroId] = new Dictionary<string, string>
    //        {
    //            {"Weapons", ""}, {"Rings", ""}, {"Necklaces", ""},
    //            {"Hat", ""}, {"Shirt", ""}, {"Pants", ""}
    //        };
    //    }
    //    return heroEquipment[heroId];
    //}

    public int GetHeroHP(string heroId) => 100; 
    public int GetHeroAttack(string heroId) => 50; // Giả định, thay bằng logic thực tế
    public object FindHeroData(string heroId) => null; // Giả định, thay bằng logic thực tế
}

[System.Serializable]
public class EquipmentSlots
{
    public Dictionary<string, string> slots;
}