using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TemplateSystems;

public class BonusItemListUI : MonoBehaviour
{
    [SerializeField] private Transform listViewContent;
    [SerializeField] private GameObject bonusItemPrefab;
    private EquipmentInfData selectedEquipment;
    private List<BonusItemUI> bonusUIItems = new List<BonusItemUI>();
    public void Show(EquipmentInfData equipment)
    {
        selectedEquipment = equipment;
        UpdateBonusListView();
    }

    private void UpdateBonusListView()
    {
        foreach (var item in bonusUIItems)
        {
            Destroy(item.gameObject);
        }
        bonusUIItems.Clear();
        var (activeList, lockedList) = DataManager.Instance.GetActiveAndLockedEquipmentInfData(selectedEquipment);
        List<BonusItemData> bonusItems = new List<BonusItemData>();

        foreach (var equip in activeList)
        {
            bonusItems.AddRange(GetBonusItems(equip, BonusItemType.Active));
        }
        foreach (var equip in lockedList)
        {
            bonusItems.AddRange(GetBonusItems(equip, BonusItemType.Lock));
        }
        foreach (var bonusItem in bonusItems)
        {
            GameObject itemObj = Instantiate(bonusItemPrefab, listViewContent);
            BonusItemUI itemUI = itemObj.GetComponent<BonusItemUI>();
            bonusUIItems.Add(itemUI);
            itemUI.Setup(bonusItem);
        }
    }

    private List<BonusItemData> GetBonusItems(EquipmentInfData equip, BonusItemType type)
    {
        List<BonusItemData> items = new List<BonusItemData>();

        if (equip.BonusDamage != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusDamage, equip.BonusDamage, equip.Rarity, type));
        if (equip.BonusAtkRate != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusAtkRate, equip.BonusAtkRate, equip.Rarity, type));
        if (equip.BonusAtkRange != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusAtkRange, equip.BonusAtkRange, equip.Rarity, type));
        if (equip.BonusCritChance != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusCritChance, equip.BonusCritChance, equip.Rarity, type));
        if (equip.BonusCritDamage != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusCritDamage, equip.BonusCritDamage, equip.Rarity, type));
        if (equip.BonusResourceGenerate != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusResourceGenerate, equip.BonusResourceGenerate, equip.Rarity, type));
        if (equip.BonusBulletSpd != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusBulletSpd, equip.BonusBulletSpd, equip.Rarity, type));
        if (equip.BonusReduceDmgFromMob != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusReduceDmgFromMob, equip.BonusReduceDmgFromMob, equip.Rarity, type));
        if (equip.BonusReduceCooldown != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusReduceCooldown, equip.BonusReduceCooldown, equip.Rarity, type));
        if (equip.BonusGoldFromBattle != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusGoldFromBattle, equip.BonusGoldFromBattle, equip.Rarity, type));
        if (equip.BonusMaxHP != 0)
            items.Add(CreateBonusItem(TypeBonus.BonusMaxHP, equip.BonusMaxHP, equip.Rarity, type));

        return items;
    }
    private BonusItemData CreateBonusItem(TypeBonus typeBonus, float value, TypeRarity rarity, BonusItemType type)
    {
        string format = DataManager.Instance.GetInfoBonusEqupment(typeBonus);
        string textInfo = string.Format(format, value);
        return new BonusItemData
        {
            Type = type,
            TextInfo = textInfo,
            Rarity = rarity
        };
    }
}
