using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TemplateSystems;
using UISystems;
public class HeroCardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconImageElemental;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private UIStarCtr _uiStarCtr; 
    [SerializeField] private DraggableCard _draggableCard; 
    [SerializeField] private GameObject _glowUnder; 
    [SerializeField] private GameObject _glowWhite; 


    private HeroCardData _cardData;
    private MergeCardHeros _mergeCardHeros;
    private int starCount;

    public HeroCardData CardData { get => _cardData; }
    public DraggableCard DraggableCard { get => _draggableCard; set => _draggableCard = value; }
    public GameObject GlowUnder { get => _glowUnder; }
    public GameObject GlowWhite { get => _glowWhite; }

    public void Setup(HeroCardData data, MergeCardHeros mergeCardHeros)
    {
        _cardData = data;
        _mergeCardHeros = mergeCardHeros;
        backgroundImage.color = DataManager.Instance.RarityTypeColorsHeroData.GetColor(data.RarityType);
        ElementalType elemental = DataManager.Instance.GetElementalHero(data.HeroType);
        if (data.CardType == TypeCard.Hero)
        {
            nameText.gameObject.SetActive(false);
            iconImageElemental.transform.parent.gameObject.SetActive(true);
            UIManager.instance.LoadIcon("Hero_" + (int)data.HeroType, iconImage);
            UIManager.instance.LoadIcon("Elemental_" + elemental.ToString(), iconImageElemental);
        }
        else
        {
            nameText.text = data.SkillData.skillName;
            UIManager.instance.LoadIcon(data.SkillData.Id, iconImage);
            nameText.gameObject.SetActive(true);
            iconImageElemental.transform.parent.gameObject.SetActive(false);
        }

        //nameText.text = data.cardName;
        // descriptionText.text = data.description;
        //menuGame.LoadIcon(cardData.cardId, iconImage);
        // Display stars
        //_uiStarCtr.DisplayStars(starCount);
        if(_draggableCard != null)
          _draggableCard.Initialize(_mergeCardHeros, _cardData);
    }

}