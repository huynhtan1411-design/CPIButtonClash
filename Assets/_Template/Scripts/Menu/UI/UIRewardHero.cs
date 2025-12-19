using System.Collections;
using System.Collections.Generic;
using UISystems;
using UnityEngine;
using TMPro;
using TemplateSystems;
using UnityEngine.UI;
public class UIRewardHero : MonoBehaviour
{
    [SerializeField] private UIElemental UIElemental;
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private Image iconImage;
    public void ShowRewardHeroById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Hero ID is null or empty!");
            return;
        }
        //Hero3dUI.Instance.ShowHeroById(id);
        //var heroInfo = DataManager.Instance.GetInfoDataHero(id);
        var buildingData = DataManager.Instance.GetBuildingDataByID(id);
        var buildingBaseData = buildingData.GetLevelData(1);
        if (buildingData == null)
            Debug.LogError("Null");
        iconImage.sprite = buildingBaseData.icon;
        txtName.text = buildingData.nameBase;
    }
    public void OnClose()
    {
        UIManager.instance.SetMenu();
    }
}
