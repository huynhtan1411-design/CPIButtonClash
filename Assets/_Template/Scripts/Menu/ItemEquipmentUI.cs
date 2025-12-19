using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UISystems;
using System;
using TemplateSystems;

public class ItemEquipmentUI : MonoBehaviour
{
    [SerializeField] private Image iconImage; 
    [SerializeField] private Image bgImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private UIStarCtr uIStarCtr;
    [SerializeField] private Button button;
    [SerializeField] private CLHomaUINoticeItem noticeItem;
    private EquipmentItemData itemEquipmentData;
    public EquipmentItemData ItemEquipmentData { get => itemEquipmentData; set => itemEquipmentData = value; }
    public void SetItem(EquipmentItemData equipment,bool isEquipped, Action<EquipmentItemData, bool> onClick)
    {
        if (equipment != null && !string.IsNullOrEmpty(equipment.Id))
        {
            EquipmentInfData infData = DataManager.Instance.GetEquipmentInfo(equipment.Id);
            equipment.Rarity = infData.Rarity;
            gameObject.SetActive(true);
            itemEquipmentData = equipment;
            levelText.text = "Lv " + (equipment.Level + 1);
            bgImage.color = DataManager.Instance.RarityTypeColorsData.GetColor(equipment.Rarity);
            int star =  DataManager.Instance.GetStar(infData.Rarity);
            if(star>0)
            {
              uIStarCtr.DisplayStars(star);
                 uIStarCtr.gameObject.SetActive(true);
            }
            else
              uIStarCtr.gameObject.SetActive(false);
            UIManager.instance.LoadIconEquipments(equipment.Id, iconImage);
            if(onClick != null)
              button.onClick.AddListener(() => onClick(itemEquipmentData, isEquipped));

            if (noticeItem != null)
                noticeItem.UpdateTrigger(NoticeKey.Equipment_Item.ToString() + itemEquipmentData.Id);
        }
        else 
            gameObject.SetActive(false);

    }

   public bool IsSlotEmpty()
    {
        return itemEquipmentData == null;
    }

    public void Clear()
    {
        itemEquipmentData = null;
        gameObject.SetActive(false);
    }
    public void UpdateNotice()
    {
        if (DataManager.Instance == null) return;
        bool isNotice = DataManager.Instance.CanMergeItems(itemEquipmentData.Id);
        if (isNotice)
            UINoticeManager<string>.Instance.UpdateInfo(NoticeKey.Equipment_Item.ToString() + itemEquipmentData.Id, NoticeStatus.Red);
        else
            UINoticeManager<string>.Instance.UpdateInfo(NoticeKey.Equipment_Item.ToString() + itemEquipmentData.Id, NoticeStatus.None);
    }
}