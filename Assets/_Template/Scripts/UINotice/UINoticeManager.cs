using System.Collections.Generic;

public enum NoticeStatus
    {
        None,
        Green,
        Yellow,
        Red
    }
    public class NoticeInfo
    {
        public NoticeStatus Status;
        public int Number;
    }

    public class UINoticeManager<T>
    {
        protected static UINoticeManager<T> _instance = null;
        protected Dictionary<T, List<UINoticeItem<T>>> _dctUIItems = new Dictionary<T, List<UINoticeItem<T>>>();
        protected Dictionary<T, NoticeInfo> _dctInfo = new Dictionary<T, NoticeInfo>();
        public static UINoticeManager<T> Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UINoticeManager<T>();
                return _instance;
            }
        }

        public NoticeInfo GetNotice(T trigger)
        {
            NoticeInfo info = null;
            _dctInfo.TryGetValue(trigger, out info);
            return info;
        }
        public void AddUIItem(UINoticeItem<T> item)
        {
            var trigger = item.Trigger;
            List<UINoticeItem<T>> lst = null;
            if (!_dctUIItems.TryGetValue(trigger, out lst))
            {
                lst = new List<UINoticeItem<T>>();
                _dctUIItems[trigger] = lst;
            }
            lst.Add(item);
            item.Show(GetNotice(trigger));
        }

        public void RemoveUIItem(UINoticeItem<T> item)
        {
            var trigger = item.Trigger;
            List<UINoticeItem<T>> lst = null;
            if (_dctUIItems.TryGetValue(trigger, out lst))
            {
                lst.Remove(item);
                item.gameObject.SetActive(false);
            }
        }

        public void UpdateInfo(T trigger, NoticeStatus status, int number = 0)
        {
            bool hasChange = false;
            NoticeInfo info = null;
            if (!_dctInfo.TryGetValue(trigger, out info))
            {
                info = new NoticeInfo { Status = status, Number = number };
                _dctInfo[trigger] = info;
                hasChange = true;
            }
            else
            {
                if (info.Status != status || info.Number != number)
                {
                    info.Status = status;
                    info.Number = number;
                    hasChange = true;
                }
            }

            if (hasChange)
            {
                List<UINoticeItem<T>> lst = null;
                if (_dctUIItems.TryGetValue(trigger, out lst))
                {
                    foreach (var uiItem in lst)
                    {
                        if (uiItem != null)
                            uiItem.Show(info);
                    }
                }
            }
        }
    }
