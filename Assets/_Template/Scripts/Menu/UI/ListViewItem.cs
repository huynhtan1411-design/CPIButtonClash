using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using UISystems;
using System.Security.Principal;

public class ListViewItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private UIStarCtr uIStarCtr; 

    private string id;
    private int countStar;

    public void Init(string id, int countStar)
    {
        this.id = id;
        this.countStar = countStar;
        UIManager.instance.MenuGameCtr.LoadIcon(id, iconImage);
        gameObject.SetActive(true);
        if(uIStarCtr != null)
          uIStarCtr.DisplayStars(this.countStar);
    }
}