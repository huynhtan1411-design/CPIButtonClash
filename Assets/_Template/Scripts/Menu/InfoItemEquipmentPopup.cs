using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UISystems;
using TemplateSystems;
using System.Collections.Generic;

public class InfoItemEquipmentPopup : MonoBehaviour
{
    [SerializeField] private ItemEquipmentUI itemlUI;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI LevelText;
    [SerializeField] private TextMeshProUGUI desText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    //Bonus
    [SerializeField] private BonusItemListUI bonusItemListUI; 
    [SerializeField] private Transform listViewContent; 
    [SerializeField] private GameObject bonusItemPrefab;
    private EquipmentInfData selectedEquipment;
    private List<BonusItemUI> bonusUIItems = new List<BonusItemUI>();
    //
    private EquipmentItemData equipmentItemData;
    private EquipmentUI equipmentUI;
    private bool isEquipped;
    private int coin = 0;
    private int price = 0;
    private int damage = 0;
    private int levelMax = 0;

    private void OnDisable()
    {
        ClosePopup();
    }

    public void Setup(EquipmentItemData equipData, bool isEquipped, EquipmentUI equipmentUI)
    {
        EquipmentInfData infData = DataManager.Instance.GetEquipmentInfo(equipData.Id);
        Color colorRarity = DataManager.Instance.RarityTypeColorsData.GetColor(infData.Rarity);
        RankUpBonusInfoData bonus =  DataManager.Instance.GetRankUpBonusInfoData(equipData.Rarity);
        if(bonus != null)
        levelMax = bonus.UpgradeLvMax;
        equipData.Rarity = infData.Rarity;
        itemlUI.SetItem(equipData, false, null);
        equipmentItemData = equipData;
        this.isEquipped = isEquipped;
        this.equipmentUI = equipmentUI;
        nameText.text = infData.Name;
        typeText.text = infData.Rarity.ToString();
        typeText.color = colorRarity;
        hpText.text = equipData.HP.ToString();
        damageText.text = equipData.Damage.ToString();
        nameText.text = infData.Name;
        LevelText.text = (equipData.Level +1) + "/" + levelMax;
        desText.text = " " + infData.Description;
        price = DataManager.Instance.GetPriceInfo(equipmentItemData.Level + 1);
        priceText.text = price.ToString();
        coinText.text = DataManager.COINS.ToString();
        if (DataManager.COINS < price)
            coinText.color = Color.red;
        else
            coinText.color = Color.white;
        actionButtonText.text = isEquipped ? "Unequip" : "Equip";
        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        levelUpButton.onClick.RemoveAllListeners();
        levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
        CheckLevelMax();
        bonusItemListUI.Show(infData);
        UIManager.instance.CloseTutorial(TutorialStepID.Pick_Item);
        UIManager.instance.SetTutorial(TutorialStepID.Equip_Item);
    }

    private void OnEquipButtonClicked()
    {
        if (isEquipped)
        {

            equipmentUI.UnequipItem(equipmentItemData);
        }
        else
        {
            UIManager.instance.CloseTutorial(TutorialStepID.Equip_Item);
            UIManager.instance.SetTutorial(TutorialStepID.Talent_Menu);
            equipmentUI.EquipItem(equipmentItemData);
        }
        gameObject.SetActive(false);
        Audio_Manager.instance.play("Click");
    }

    private void OnLevelUpButtonClicked()
    {
 UnityEngine.Debug.LogWarning(equipmentItemData.Level +" Max Level " + levelMax);
         if (CheckLevelMax()) 
        {
            return;
        }
        if (DataManager.COINS >= price) 
        {
            DataManager.AddCoins(-price);
            equipmentItemData.Level++;
            equipmentItemData.HP++;
            if ((int)equipmentItemData.Rarity >= 6)
                equipmentItemData.Damage += 2;
            else
                equipmentItemData.Damage += 1;
            Setup(equipmentItemData, this.isEquipped, equipmentUI);

            DataManager.Instance.SaveData();

            Audio_Manager.instance.play("Click");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Not enough coins to upgrade!");
        }

    }
    bool CheckLevelMax()
    {
        if ((equipmentItemData.Level +1) >= levelMax) 
        {
            priceText.transform.parent.gameObject.SetActive(false);
            levelUpButton.interactable = false;
            return true;
        }
        else
        {
            priceText.transform.parent.gameObject.SetActive(true);
            levelUpButton.interactable = true;

        }
      return false;
    }
    public void ClosePopup()
    {
        equipmentUI.UpdateUI();
    }
}
