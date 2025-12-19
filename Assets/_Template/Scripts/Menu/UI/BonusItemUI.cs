using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TemplateSystems;

public class BonusItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Image iconActive;
    [SerializeField] private Image iconLock;

    public void Setup(BonusItemData bonusItem)
    {
        gameObject.SetActive(true);
        infoText.text = bonusItem.TextInfo;
        Color rarityColor = DataManager.Instance.GetColor(bonusItem.Rarity);

        if (bonusItem.Type == BonusItemType.Active)
        {
            iconActive.gameObject.SetActive(true);
            iconLock.gameObject.SetActive(false);
            iconActive.color = rarityColor;
            infoText.color = Color.white;
        }
        else //Lock
        {
            iconActive.gameObject.SetActive(false);
            iconLock.gameObject.SetActive(true);
            iconLock.color = rarityColor;
            infoText.color = Color.gray;
        }
    }
}
