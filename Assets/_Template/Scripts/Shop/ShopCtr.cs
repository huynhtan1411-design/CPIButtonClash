using System.Collections.Generic;
using UnityEngine;

public class ShopCtr : MonoBehaviour
{
    [SerializeField] List<UIShopBtn> uIShopBtns = null;
    private void Start()
    {
        Setup();
    }

    public void Setup()
    {
        foreach (var btn in uIShopBtns)
        {
            btn.gameObject.SetActive(true);
        }
    }
}
