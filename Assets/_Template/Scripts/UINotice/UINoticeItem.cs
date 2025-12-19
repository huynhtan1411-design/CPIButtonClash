using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UINoticeItem<T> : MonoBehaviour
{
    [Header("Trigger")]
    public T Trigger;
    [Header("UI")]
    [SerializeField] private Image _imgIcon = null;
    [SerializeField] private TextMeshProUGUI _txt = null;
    [Header("Sprite")]
    [SerializeField] private Sprite _sprGreen = null;
    [SerializeField] private Sprite _sprYellow = null;
    [SerializeField] private Sprite _sprRed = null;
    [Space]
    [SerializeField] private bool _runInAwake = true;

    private void Awake()
    {
        if (_runInAwake)
            UINoticeManager<T>.Instance.AddUIItem(this);
    }

    private void OnDestroy()
    {
        if (UINoticeManager<T>.Instance != null)
            UINoticeManager<T>.Instance.RemoveUIItem(this);
    }

    public void UpdateTrigger(T newTrigger)
    {
        UINoticeManager<T>.Instance.RemoveUIItem(this);
        Trigger = newTrigger;
        UINoticeManager<T>.Instance.AddUIItem(this);
    }

    public void Show(NoticeInfo info)
    {
        if (info == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(info.Status != NoticeStatus.None);
        if (_txt)
            _txt.text = info.Number.ToString();
        switch (info.Status)
        {
            case NoticeStatus.Green:
                _imgIcon.sprite = _sprGreen;
                break;
            case NoticeStatus.Yellow:
                _imgIcon.sprite = _sprYellow;
                break;
            case NoticeStatus.Red:
                _imgIcon.sprite = _sprRed;
                break;
        }
    }
}