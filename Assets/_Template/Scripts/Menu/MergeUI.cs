using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UISystems;
using TemplateSystems;
using TMPro;
using UnityEngine.UI;

public class MergeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private ItemEquipmentUI[] slots; 
    [SerializeField] private GameObject[] slotsBG; 
    [SerializeField] private Transform availableItemsContent; // Scroll view content for available items
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject itemEquipmentPrefab;
     [SerializeField] private EquipmentUI equipmentUI;
    [SerializeField] private MergeCompletePopup mergeCompletePopup;
    private List<EquipmentItemData> availableItems; 
    private List<ItemEquipmentUI> equipmentItems = new List<ItemEquipmentUI>();
    private EquipmentItemData[] mergeItems = new EquipmentItemData[4]; // [0]: result, [1]: first, [2]: second, [3]: third
    private EquipmentItemData newItem = null;
    void OnEnable()
    {
        availableItems = new List<EquipmentItemData>(DataManager.Instance.BagEquipmentData);
        InitializeSlots();
        PopulateEquipmentList();
        mergeButton.gameObject.SetActive(false);
        
    }
    void OnDisable()
    {
        equipmentUI.UpdateUI();
    }
    private void Start()
    {

    }

    private void InitializeSlots()
    {
        slotsBG[0].SetActive(true); // Result slot
        slotsBG[1].SetActive(true); // First merge slot
        slotsBG[2].SetActive(false); // Second merge slot, initially inactive
        slotsBG[3].SetActive(false); // Third merge slot, initially inactive
        ClearSlot(slots[0]);
        ClearSlot(slots[1]);
        ClearSlot(slots[2]);
        ClearSlot(slots[3]);
    }
   private void PopulateEquipmentList()
    {
        foreach (var item in equipmentItems)
        {
            Destroy(item.gameObject);
        }
        equipmentItems.Clear();
        availableItems = DataManager.Instance.SortBagEquipmentData(availableItems);
        foreach (var equip in availableItems)
        {
            if(equip.Rarity == TypeRarity.Immortal2)
               continue;
            var itemObj = Instantiate(itemEquipmentPrefab, availableItemsContent);
            var itemUI = itemObj.GetComponent<ItemEquipmentUI>();
            itemObj.SetActive(true);
            itemUI.SetItem(equip, false, OnSelectItem);
            equipmentItems.Add(itemUI);
            itemUI.UpdateNotice();
        }
    }

    private void OnSelectItem(EquipmentItemData equipment, bool isEquipped)
    {
        if (IsSlotEmpty(slots[1])) 
        {
            AssignToSlot(1, equipment);
            slotsBG[2].SetActive(true); 
            slotsBG[3].SetActive(true); 
            //string originalId = mergeItems[1].Id;
            //string newItemId = GetNewItemId(originalId);
        }
        else if (IsSlotEmpty(slots[2]) && IsMatchingTypeAndRarity(equipment, mergeItems[1]))
        {
              Debug.LogError("2");
            AssignToSlot(2, equipment);
            slotsBG[3].SetActive(true);
        }
        else if (IsSlotEmpty(slots[3]) && IsMatchingTypeAndRarity(equipment, mergeItems[1]))
        {
            AssignToSlot(3, equipment);
        }
        else
        {
   
        }
        UpdateMergeButtonState();
    }

    private bool IsMatchingTypeAndRarity(EquipmentItemData item, EquipmentItemData reference)
    {
        if (reference == null || item == null) return false;
        return item.Id == reference.Id;
        //return DataManager.Instance.GetEquipmentType(item.Id) == DataManager.Instance.GetEquipmentType(reference.Id) &&
        //       item.Rarity == reference.Rarity;
    }

    private void AssignToSlot(int slotIndex, EquipmentItemData equipment)
    {
        availableItems.Remove(equipment);
        mergeItems[slotIndex] = equipment;
        slots[slotIndex].SetItem(equipment, true, OnRemoveFromSlot);
        newItem = GetNewItemData();
        if(newItem != null)
          UpdateInfo();
        else
          Debug.LogError("newItem null");
        //backButton.gameObject.SetActive(false);
        PopulateEquipmentList();
        SortList();
    }

    private void OnRemoveFromSlot(EquipmentItemData equipment, bool isEquipped)
    {
        if(equipment == null)
         return;
        Debug.LogError("OnRemoveFromSlot " + equipment.Id);
        for (int i = 1; i <= 3; i++)
        {
            if (mergeItems[i] == equipment)
            {
                availableItems.Add(equipment);
                mergeItems[i] = null;
                ClearSlot(slots[i]);

                if (i == 1)
                {
                    // remove slot[1], reset all
                    if(mergeItems[2] != null)
                     availableItems.Add(mergeItems[2]);
                    if(mergeItems[3] != null)
                     availableItems.Add(mergeItems[2]);
                    ClearSlot(slots[0]);
                    ClearSlot(slots[2]);
                    ClearSlot(slots[3]);
                    newItem = new EquipmentItemData();
                    UpdateInfo();
                    slotsBG[2].SetActive(false);
                    slotsBG[3].SetActive(false);
                    mergeItems[2] = null;
                    mergeItems[3] = null;
                    backButton.gameObject.SetActive(true);
                     PopulateEquipmentList();
                }
                else
                {
                    PopulateEquipmentList();
                    SortList();
                }
                break;
            }
        }

        UpdateMergeButtonState();
    }

    private void UpdateMergeButtonState()
    {
        bool canMerge = mergeItems[1] != null && mergeItems[2] != null && mergeItems[3] != null &&
                       IsMatchingTypeAndRarity(mergeItems[2], mergeItems[1]) && 
                       IsMatchingTypeAndRarity(mergeItems[3], mergeItems[1]);
        mergeButton.gameObject.SetActive(canMerge);
    }
    EquipmentItemData GetNewItemData()
    {
        string originalId = mergeItems[1].Id;
        string newItemId = GetNewItemId(originalId);

        if (mergeItems[1] == null)
        {
            return null;
        }
        EquipmentItemData infHighData = GetHighestLevelItem();
        TypeRarity RarityNew  = GetNextRarity(infHighData.Rarity);
        RankUpBonusInfoData bonus =  DataManager.Instance.GetRankUpBonusInfoData(RarityNew);
        EquipmentItemData newInfData = new EquipmentItemData
       {
        Id = newItemId,
        Level = infHighData.Level,
        HP = infHighData.HP + bonus.HPRankUp,
        Damage =  infHighData.Damage + bonus.DamageRankUp,
        Rarity = RarityNew
        };
        return newInfData;
    }
    private void UpdateInfo()
    {
        mergeItems[0] = newItem;
        hpText.text = newItem.HP.ToString();
        damageText.text = newItem.Damage.ToString();
        slots[0].SetItem(newItem, false, null);
        
    }
    public void OnMergeButtonClicked()
    {
        MergeItems();
    }
    public void OnBackMergeButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void MergeItems()
    {
        Debug.LogError("MergeItems");
        DataManager.Instance.BagEquipmentData.Add(newItem);

        DataManager.Instance.BagEquipmentData.Remove(mergeItems[1]);
        DataManager.Instance.BagEquipmentData.Remove(mergeItems[2]);
        DataManager.Instance.BagEquipmentData.Remove(mergeItems[3]);

        mergeCompletePopup.Setup(newItem);
        newItem = new EquipmentItemData();
        UpdateInfo();
        InitializeSlots();
        mergeItems[0] = null;
        mergeItems[1] = null;
        mergeItems[2] = null;
        mergeItems[3] = null;
        slots[2].gameObject.SetActive(false);
        slots[3].gameObject.SetActive(false);

        availableItems = new List<EquipmentItemData>(DataManager.Instance.BagEquipmentData);
        PopulateEquipmentList();

        DataManager.Instance.SaveData();
        PopulateEquipmentList();
    }

    private string GetNewItemId(string originalId)
    {
        string middleTwoDigits = originalId.Substring(2, 2);
        int middleNumber = int.Parse(middleTwoDigits);
        int newMiddleNumber = middleNumber + 1;
        string newMiddleTwoDigits = newMiddleNumber.ToString("D2");
        return originalId.Substring(0, 2) + newMiddleTwoDigits + originalId.Substring(4);
    }

    private TypeRarity GetNextRarity(TypeRarity currentRarity)
    {

        TypeRarity[] rarities = (TypeRarity[])Enum.GetValues(typeof(TypeRarity));
        int currentIndex = Array.IndexOf(rarities, currentRarity);
        if (currentIndex < rarities.Length - 1)
        {
            return rarities[currentIndex + 1]; 
        }
        return currentRarity; 
    }

private EquipmentItemData GetHighestLevelItem()
    {
    EquipmentItemData highestLevelItem = null;
    int maxLevel = -1; 

    foreach (var item in mergeItems.Skip(1)) 
    {
        if (item != null && item.Level > maxLevel)
        {
            highestLevelItem = item;
            maxLevel = item.Level;
        }
    }

    if (highestLevelItem == null)
    {
        Debug.LogWarning("No valid items found to determine highest level.");
    }

    return highestLevelItem;
    }
    private void SortList()
    {
        foreach(var item in equipmentItems)
        {
            if(IsMatchingTypeAndRarity(item.ItemEquipmentData, mergeItems[1]))
            item.gameObject.SetActive(true);
            else
            item.gameObject.SetActive(false);
        }

    }
    private bool IsSlotEmpty(ItemEquipmentUI slotUI)
    {
        return slotUI.IsSlotEmpty();
    }

    private void ClearSlot(ItemEquipmentUI slotUI)
    {
        slotUI.Clear();
    }
}
