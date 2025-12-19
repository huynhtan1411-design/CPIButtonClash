using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TemplateSystems;
using System.Linq;
using UISystems;
using System.Collections;
using Unity.VisualScripting;
public class EquipmentUI : MonoBehaviour
{
    [SerializeField] private RawImage heroImage;
    [SerializeField] private ItemEquipmentUI[] leftSlots; // Weapons, Rings, Necklaces
    [SerializeField] private ItemEquipmentUI[] rightSlots; // Hat, Shirt, Glove
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private Button prevHeroButton;
    [SerializeField] private Button nextHeroButton;
    [SerializeField] private LockNotiUI lockNotiUI;
    [SerializeField] private Transform equipmentListContent; // ScrollView Content
    [SerializeField] private GameObject itemEquipmentPrefab; // Prefab cho ItemEquipment
    [SerializeField] private GameObject infoItemEquipmentPopup; 
    [SerializeField] private GameObject mergePopup;
    [SerializeField] private GameObject equipmentContenObj;
    private int currentHeroIndex = 0;
    private List<ItemEquipmentUI> equipmentItems = new List<ItemEquipmentUI>();
    private string currentHeroId;
    private EquipmentHeroData equipmentHeroDataCur = new EquipmentHeroData();
    private void Awake()
    {

    }

    private void Start()
    {
         infoItemEquipmentPopup.SetActive(false);
    }
    
    private void OnEnable()
    {
        //UIManager.instance.SetLoading(1, () =>
        //{

        //});
        //DataManager.Instance.LoadData();
        Setup("0");
    }

    public void Setup(string heroId)
    {
        currentHeroId = heroId;
        UpdateHeroDisplay();
        PopulateEquipmentList();
    }

    public void UpdateUI()
    {
        UpdateHeroDisplay();
        PopulateEquipmentList();
    }

    private void UpdateHeroDisplay()
    {
        if (DataManager.Instance.HeroEquipmentData.ContainsKey(currentHeroId))
            equipmentHeroDataCur = DataManager.Instance.HeroEquipmentData[currentHeroId];
        else
        {
            DataManager.Instance.HeroEquipmentData[currentHeroId] = new EquipmentHeroData();
            equipmentHeroDataCur = DataManager.Instance.HeroEquipmentData[currentHeroId];
        }

        HerosInfoData hero = DataManager.Instance.GetInfoDataHero(currentHeroId);
        heroNameText.text = hero.Name;
        if (hpText.text != null)
        hpText.text = " " + DataManager.Instance.GetHeroHP(currentHeroId);
        if (attackText.text != null)
            attackText.text = " " + DataManager.Instance.GetHeroAttack(currentHeroId);
        UIManager.instance.UpdateCoins();
        var heroEquipment = DataManager.Instance.HeroEquipmentData;
        if (heroEquipment.ContainsKey(currentHeroId))
        {
            var equipment = heroEquipment[currentHeroId];

            UpdateSlot(leftSlots[0], equipment.Equipment.ContainsKey(TypeEquipment.SubWeapon.ToString()) ? equipment.Equipment[TypeEquipment.SubWeapon.ToString()] : null);
            UpdateSlot(leftSlots[1], equipment.Equipment.ContainsKey(TypeEquipment.Ring.ToString()) ? equipment.Equipment[TypeEquipment.Ring.ToString()] : null);
            UpdateSlot(leftSlots[2], equipment.Equipment.ContainsKey(TypeEquipment.Necklace.ToString()) ? equipment.Equipment[TypeEquipment.Necklace.ToString()] : null);

            UpdateSlot(rightSlots[0], equipment.Equipment.ContainsKey(TypeEquipment.Hat.ToString()) ? equipment.Equipment[TypeEquipment.Hat.ToString()] : null);
            UpdateSlot(rightSlots[1], equipment.Equipment.ContainsKey(TypeEquipment.Shirt.ToString()) ? equipment.Equipment[TypeEquipment.Shirt.ToString()] : null);
            UpdateSlot(rightSlots[2], equipment.Equipment.ContainsKey(TypeEquipment.Gloves.ToString()) ? equipment.Equipment[TypeEquipment.Gloves.ToString()] : null);
        }
        else
        {
            //ClearSlots(leftSlots);
            //ClearSlots(rightSlots);
        }

        UpdateHeroRenderTexture();
        CheckShowLockHeros(currentHeroId);
    }

    private void UpdateSlot(ItemEquipmentUI slotUI, EquipmentItemData equipment)
    {
        if(slotUI != null)
        slotUI.SetItem(equipment, true, OnItemClicked);
    }

    private void ClearSlots(SlotUI[] slots)
    {
        foreach (var slot in slots)
        {
            slot.image = null;
            slot.levelText.text = "";
        }
    }

    private void UpdateHeroRenderTexture()
    {
        Hero3dUI.Instance.ShowHeroById(currentHeroId);
    }

    private void PopulateEquipmentList()
    {
        foreach (var item in equipmentItems)
        {
            Destroy(item.gameObject);
        }
        equipmentItems.Clear();
        var bagEquipmentData = DataManager.Instance.BagEquipmentData;
        var availableEquipment = new List<EquipmentItemData>();
        bagEquipmentData = DataManager.Instance.SortBagEquipmentData(bagEquipmentData);
        foreach (var equip in bagEquipmentData)
        {
            var itemObj = Instantiate(itemEquipmentPrefab, equipmentListContent);
            var itemUI = itemObj.GetComponent<ItemEquipmentUI>();
            itemObj.SetActive(true);
            itemUI.SetItem(equip, false, OnItemClicked);
            equipmentItems.Add(itemUI);
        }
    }
    private void OnItemClicked(EquipmentItemData equipment, bool isEquipped)
    {
        infoItemEquipmentPopup.SetActive(true);
        var popup = infoItemEquipmentPopup.GetComponent<InfoItemEquipmentPopup>();
        popup.Setup(equipment, isEquipped, this);
    }

    public void EquipItem(EquipmentItemData equipment)
    {
        TypeEquipment typeEquipment = DataManager.Instance.GetEquipmentType(equipment.Id);
         string key = typeEquipment.ToString();
        if(equipmentHeroDataCur.Equipment.ContainsKey(key))
          DataManager.Instance.BagEquipmentData.Add(equipmentHeroDataCur.Equipment[key]);
        equipmentHeroDataCur.Equipment[key] = equipment;
        DataManager.Instance.BagEquipmentData.Remove(equipment);
        DataManager.Instance.SaveData();
        UpdateHeroDisplay();
        PopulateEquipmentList();
    }

    public void UnequipItem(EquipmentItemData equipment)
    {
        if (equipmentHeroDataCur != null)
        {
            TypeEquipment typeEquipment = DataManager.Instance.GetEquipmentType(equipment.Id);
            string key = typeEquipment.ToString();
            if(equipmentHeroDataCur.Equipment.ContainsKey(key))
               equipmentHeroDataCur.Equipment[key] = null;
            DataManager.Instance.BagEquipmentData.Add(equipment);
            DataManager.Instance.SaveData();
        }
        UpdateHeroDisplay();
        PopulateEquipmentList();
    }

    private void CheckShowLockHeros(string id)
    {
        HerosInfoData hero = DataManager.Instance.GetInfoDataHero(id);
        int level = DataManager.Instance.GetLevel();
        Debug.LogError("level" + level);
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
        UpdateHeroDisplay();
    }

    public void NextHero()
    {
        var heroes = DataManager.Instance.HerosInfoData.Data;
        if (heroes.Count == 0) return;
        currentHeroIndex = (currentHeroIndex + 1) % heroes.Count;
        currentHeroId = heroes[currentHeroIndex].Id;
        UpdateHeroDisplay();
    }

     public void OpenPoupMerge()
     {
        mergePopup.SetActive(true);
        
     }
}
[System.Serializable]
public class SlotUI
{
    public Image image;
    public TMP_Text levelText;
}