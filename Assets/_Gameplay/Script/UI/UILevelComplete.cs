using UnityEngine;
using System.Collections.Generic;
using ButtonClash.UI;
using TemplateSystems;
using TMPro;
using UISystems;
public class UILevelComplete : MonoBehaviour
{
    [SerializeField] private UIItemList _itemList;
    [SerializeField] TextMeshProUGUI _txtChapter;
    [SerializeField] TextMeshProUGUI _textClearTime;

    //[SerializeField] private DelayedButton _delayButton;
    public void SetInfo(string info, int numSecond = 120)
    {
        _txtChapter.text = info;

        int minutes = Mathf.FloorToInt(numSecond / 60);
        int seconds = Mathf.FloorToInt(numSecond % 60);
        _textClearTime.text = UIManager.instance.UIGameplayCtr.GetSurvivalTime();

        //_delayButton.ShowButton();
    }
    public void SetupReward(int goldAmout, List<EquipmentInfData> infDatas, List<HeroCardData> heroCardDatas = null) 
    {
        List<ItemInfo> items = new List<ItemInfo>();
        items.Add(new ItemInfo
        {
            Id = "2",
            Quantity = goldAmout,
            ItemType = "Currency"
        });
        if(infDatas != null)
        {
            foreach (EquipmentInfData infData in infDatas)
            {
                items.Add(new ItemInfo
                {
                    Id = infData.ID,
                    ItemType = "Equipment",
                    ColorRarity = DataManager.Instance.RarityTypeColorsData.GetColor(infData.Rarity)
                });
            }
        }
        if (heroCardDatas != null)
        {
            foreach (var infData in heroCardDatas)
            {
                items.Add(new ItemInfo
                {
                    Id = infData.Id,
                    ItemType = "Hero",
                    ColorRarity = DataManager.Instance.RarityTypeColorsData.GetColor(TypeRarity.Common),
                    Quantity = infData.Quantity
                });
            }
        }
        _itemList.Setup(items, false);
    }
    public void SetupRewardCard(int goldAmout, List<EquipmentInfData> infDatas, List<BuildingCollectionItem> cardDatas = null)
    {
        List<ItemInfo> items = new List<ItemInfo>();
        items.Add(new ItemInfo
        {
            Id = "2",
            Quantity = goldAmout,
            ItemType = "Currency"
        });
        if (infDatas != null)
        {
            foreach (EquipmentInfData infData in infDatas)
            {
                items.Add(new ItemInfo
                {
                    Id = infData.ID,
                    ItemType = "Equipment",
                    ColorRarity = DataManager.Instance.RarityTypeColorsData.GetColor(infData.Rarity)
                });
            }
        }
        if (cardDatas != null)
        {
            foreach (var infData in cardDatas)
            {
                items.Add(new ItemInfo
                {
                    Id = infData.buildingID,
                    ItemType = "Tower",
                    ColorRarity = DataManager.Instance.RarityTypeColorsData.GetColor(TypeRarity.Common),
                    Quantity = infData.Quantity
                });
            }
        }
        _itemList.Setup(items, false);
    }
    public void OnHomeClick()
    {
        //int chapterIndexPrevious = CLHoma.GameManager.Instance.CurrentChapterIndex - 1;
 
        //string id = CLHoma.SkillManager.Instance.Database.GetHeroesUnlockedAtChapter(chapterIndexPrevious);
        List<BuildingCollectionItem> unlockCards = DataManager.Instance.UnlockBuilding();
        Debug.LogError("OnHomeClick " + unlockCards.Count);
        if (unlockCards != null && unlockCards.Count>0)
        {
            //string idHero = int.Parse(heroCards[0].Id).ToString();
            //Debug.LogError("iD: " + idHero);
            UIManager.instance.SetRewardHero(unlockCards[0].buildingID);
            Debug.LogError("unlockCards" + unlockCards[0].buildingID);
        }
        else
        {
            UIManager.instance.SetMenu();
        }
        //UIManager.instance.SetMenu();
    }
} 