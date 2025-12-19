using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LockNotiUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textNoti = null;
    public void Show(string noti)
    {
        _textNoti.text = noti;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
