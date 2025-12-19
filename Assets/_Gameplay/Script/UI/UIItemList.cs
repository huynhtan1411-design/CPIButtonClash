using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonClash.UI
{
    public class UIItemList : MonoBehaviour
    {
        [SerializeField] private Transform _container;
        [SerializeField] private UIItem _itemPrefab;
        [SerializeField] private LayoutGroup _layoutGroup;

        private readonly List<UIItem> _items = new List<UIItem>();
        public IReadOnlyList<UIItem> Items => _items;

        public void Setup(Sprite[] sprites, bool needNativeSize = false)
        {
            if (_layoutGroup != null)
                _layoutGroup.enabled = false;

            ClearItems();

            if (sprites != null)
            {
                foreach (var sprite in sprites)
                {
                    var item = Instantiate(_itemPrefab, _container);
                    item.Setup(sprite);
                    if (needNativeSize && item.Icon != null)
                        item.Icon.SetNativeSize();
                    _items.Add(item);
                }
            }

            if (_layoutGroup != null)
                _layoutGroup.enabled = true;
        }

        public void Setup(List<ItemInfo> items, bool needNativeSize = false)
        {
            if (_layoutGroup != null)
                _layoutGroup.enabled = false;

            ClearItems();

            if (items != null)
            {
                foreach (var info in items)
                {
                    var item = Instantiate(_itemPrefab, _container);
                    item.Setup(info);
                    if (needNativeSize && item.Icon != null)
                        item.Icon.SetNativeSize();
                    _items.Add(item);
                }
            }

            if (_layoutGroup != null)
                _layoutGroup.enabled = true;
        }

        private void ClearItems()
        {
            foreach (var item in _items)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _items.Clear();
        }
    }
} 