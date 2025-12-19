using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TemplateSystems;
using UISystems;
using DG.Tweening;

public class TalentCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image bgActiveImage;
    [SerializeField] private Image bgUnactiveImage;
    [SerializeField] private Button upgradeButton;

    private string talentType;

    public void Setup(string type, int currentLevel, System.Action<string> onUpgrade)
    {
        int levelIndex = currentLevel;
        int level = levelIndex + 1;
        talentType = type;
        nameText.text = type;
        levelText.text = "Lv." + levelIndex;
        int bonus = DataManager.Instance.GetTalentBonus(level, type);
        bonusText.text = "+" + bonus;

        //int nextLevel = level + 1;
        int price = 0;
        foreach (var data in DataManager.Instance.TalentsInfoData.Data)
        {
            if (data.Level == level)
            {
                price = data.Gold;
                break;
            }
        }
        priceText.text = price.ToString();

        bool canAfford = DataManager.COINS >= price;
        bgActiveImage.gameObject.SetActive(canAfford);
        bgUnactiveImage.gameObject.SetActive(!canAfford);
        upgradeButton.interactable = canAfford;

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => {
            AnimationClick();
            onUpgrade(talentType);
        });

        //UIManager.instance.LoadIcon(talentType.ToLower(), iconImage);
    }

    public void AnimationClick()
    {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.5f)
            .SetEase(Ease.OutQuad);
    }
}
