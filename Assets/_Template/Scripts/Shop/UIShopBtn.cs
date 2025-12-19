using TMPro;
using UISystems;
using UnityEngine;

public class UIShopBtn : MonoBehaviour
{
    [SerializeField] TYPELEVEL _type = TYPELEVEL.LEVEL;
    [SerializeField] TextMeshProUGUI _textPrice = null;
    [SerializeField] TextMeshProUGUI _textLevel = null;
    private int _level = 0;
    private int _levelNext = 0;
    private float _price = 0;
    private float _value = 0;
    private void OnEnable()
    {
        Setup();
    }
    public void Setup()
    {
        _level = PlayerPrefsManager.GetLevel(_type);
        _levelNext = _level + 1;
        switch (_type)
        {
            case TYPELEVEL.LEVELHp:

                break;
            case TYPELEVEL.LEVELEnergy:

                break;
            case TYPELEVEL.LEVELDmg:

                break;
            case TYPELEVEL.LEVELIncome:

                break;
        }
        _textPrice.text = _price.ToString();
        _textLevel.text = "Lv " + _levelNext.ToString();

        if (UIManager.COINS < _price)
            _textPrice.text = "<color=red>" + _price + "</color>";
    }

    private void Update()
    {

    }

    public void Buy()
    {
        if (UIManager.COINS >= _price)
        {
            UIManager.AddCoins(-(int)_price);
            _level++;
            _levelNext = _level + 1;
            PlayerPrefsManager.SaveLevel(_type, _level);
            switch (_type)
            {
                case TYPELEVEL.LEVELHp:

                    break;
                case TYPELEVEL.LEVELEnergy:

                    break;
                case TYPELEVEL.LEVELDmg:

                    break;
                case TYPELEVEL.LEVELIncome:

                    break;
            }

            _textLevel.text = "Lv " + _levelNext.ToString();
            _textPrice.text = _price.ToString();
            if (UIManager.COINS < _price)
                _textPrice.text = "<color=red>" + _price + "</color>";

        }
    }
}
