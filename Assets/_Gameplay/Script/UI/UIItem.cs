using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UISystems;
using System;
using TemplateSystems;

namespace ButtonClash.UI
{
    public class ItemInfo
    {
        public string Id;
        public string ItemType;
        public double Quantity;
        public Color ColorRarity;
    }
    public class UIItem : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _bg;
        [SerializeField] private TextMeshProUGUI _txtName;
        [SerializeField] private TextMeshProUGUI _txtQuantity;
        [SerializeField] private string _strPrefix = "+";
        [SerializeField] private string _strSuffix = "";
        [SerializeField] private int _valueThreshold = 1;
        [SerializeField] private Sprite _defaultSprite;
        public string Id { get; private set; }
        public double Quantity { get; private set; }
        public string ItemType { get; private set; }
        public Image Icon => _icon;
        public TextMeshProUGUI TxtName => _txtName;
        public TextMeshProUGUI TxtQuantity => _txtQuantity;

        public virtual void Setup(ItemInfo info)
        {
            gameObject.SetActive(true);
            Id = info.Id;
            Quantity = info.Quantity;
            ItemType = info.ItemType;

            if (_txtName != null)
            {
                _txtName.text = info.Id; // You can customize this based on your needs
            }
            if(_icon != null)
            {

                if (ItemType.Equals("Equipment"))
                {
                    if (Id == null)
                        _icon.sprite = _defaultSprite;
                    else
                        UIManager.instance.LoadIconEquipments(Id, _icon);
                    _bg.color = info.ColorRarity;
                }
                else if(ItemType.Equals("Hero"))
                {
                    if (Id == null)
                        _icon.sprite = _defaultSprite;
                    else
                        UIManager.instance.LoadIcon("Hero_"+int.Parse(Id), _icon);
                    _bg.color = info.ColorRarity;
                }
                else if (ItemType.Equals("Tower"))
                {
                    if (Id == null)
                        _icon.sprite = _defaultSprite;
                    else
                    {
                        var buildingData = DataManager.Instance.GetBuildingDataByID(Id);
                        if (buildingData == null)
                            Debug.LogError("Null");
                        _icon.sprite = buildingData.GetLevelData(1).icon;
                    }
                    _bg.color = info.ColorRarity;
                }
                else
                    UIManager.instance.LoadIcon(Id, _icon);
            }
            if (_txtQuantity != null)
            {
                if (Mathf.Abs((float)Quantity) >= _valueThreshold)
                {
                    _txtQuantity.gameObject.SetActive(true);
                    string strQuantity = Quantity.ToString();
                    if (Quantity > 0)
                        _txtQuantity.text = string.Format("{0}{1}{2}", _strPrefix, strQuantity, _strSuffix);
                    else
                        _txtQuantity.text = strQuantity;
                }
                else
                {
                    _txtQuantity.gameObject.SetActive(false);
                }
            }
        }
        public virtual void Setup(Sprite customSprite, string name = "", int quantity = 0)
        {
            gameObject.SetActive(true);
            if(customSprite != null && _icon != null)
            {
                _icon.sprite = customSprite;
            }

            Quantity = quantity;

            if(_txtName != null)
            {
                _txtName.text = name;
            }

            SetupQuantity();
        }

        private void SetupQuantity()
        {
            if (_txtQuantity == null)
                return;

            if (Quantity >= _valueThreshold)
            {
                _txtQuantity.gameObject.SetActive(true);
                string strQuantity = Quantity.ToString();
                _txtQuantity.text = string.Format("{0}{1}{2}", _strPrefix, strQuantity, _strSuffix);
            }
            else
            {
                _txtQuantity.gameObject.SetActive(false);
            }
        }
    }
} 