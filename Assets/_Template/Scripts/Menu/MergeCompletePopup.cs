using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UISystems;
using TemplateSystems;

public class MergeCompletePopup : MonoBehaviour
{
  [SerializeField] private ItemEquipmentUI itemlUI;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private BonusItemListUI bonusItemListUI;

    private void OnDisable()
    {
        ClosePopup();
    }

    public void Setup(EquipmentItemData equipData)
    {
        gameObject.SetActive(true);
        EquipmentInfData infData = DataManager.Instance.GetEquipmentInfo(equipData.Id);
        Color colorRarity = DataManager.Instance.RarityTypeColorsData.GetColor(infData.Rarity);
        equipData.Rarity = infData.Rarity;
        itemlUI.SetItem(equipData, false, null);
        nameText.text = infData.Name;
        typeText.text = infData.Rarity.ToString();
        typeText.color = colorRarity;
        hpText.text = equipData.HP.ToString();
        damageText.text = equipData.Damage.ToString();
        nameText.text = infData.Name;
        bonusItemListUI.Show(infData);
    }
    public void ClosePopup()
    {
       
    }
}
