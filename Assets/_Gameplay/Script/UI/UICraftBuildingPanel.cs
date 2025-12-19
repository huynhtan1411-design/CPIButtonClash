using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLHoma.Combat;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Events;
using TemplateSystems;
namespace WD
{
    public class UICraftBuildingPanel : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private UIBuildingItem buildingItemPrefab;
        [SerializeField] private RectTransform tabsparent;

        private List<UIBuildingItem> spawnedItems = new List<UIBuildingItem>();

        public UnityEvent OnShowedEvent;

        public void Show(BuildingType type)
        {
            AnimationPopup();
            if (type == BuildingType.Base || type == BuildingType.Wall)
            {
                tabsparent.gameObject.SetActive(false);
            }
            else
            {
                tabsparent.gameObject.SetActive(true);
                OnShowedEvent?.Invoke();
            }
            DisplayBuildingsByType(type);
        }
        public void Show(BuildingData cf, int level)
        {
            AnimationPopup();
            tabsparent.gameObject.SetActive(false);
            DisplayUpgradeBuilding(cf, level);
        }
        private void AnimationPopup()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            transform.localPosition = new Vector3(0, -400f, 0);
            Sequence sequence = DOTween.Sequence();

            sequence.Append(transform.DOLocalMoveY(0f, 0.3f).SetEase(Ease.OutBack))
                   .Join(canvasGroup.DOFade(1f, 0.3f))
                   .OnComplete(() =>
                   {
                       canvasGroup.interactable = true;
                       canvasGroup.blocksRaycasts = true;
                   });

            Audio_Manager.instance.play("sfx_ui_popup");
        }
        private void ClearSpawnedItems()
        {
            foreach (var item in spawnedItems)
            {
                Destroy(item.gameObject);
            }
            spawnedItems.Clear();
        }

        private void DisplayBuildingsByType(BuildingType type)
        {
            ClearSpawnedItems();

            foreach (var config in TemplateSystems.DataManager.Instance.GetUnlockedBuildings())
            {
                if (config.Type == type)
                {
                    var buildingItem = Instantiate(buildingItemPrefab, contentContainer);
                    buildingItem.SetupInfo(config.GetLevelDataUpgrade(1), 1);
                    buildingItem.OnBuildClickEvent += () =>
                    {
                        //BuildingManager.Instance.CurrentActiceSlotInteractive.BuildTower(config);
                        OnCloseClick();
                    };
                    spawnedItems.Add(buildingItem);
                }
            }
        }
        private void DisplayUpgradeBuilding(BuildingData config, int currentLevel)
        {
            ClearSpawnedItems();

            var buildingItem = Instantiate(buildingItemPrefab, contentContainer);
            buildingItem.SetupUpgradeInfo(config.GetLevelDataUpgrade(currentLevel), config.GetLevelDataUpgrade(currentLevel+1), currentLevel);
            buildingItem.OnBuildClickEvent += () =>
            {
                //BuildingManager.Instance.CurrentActiceSlotInteractive.BuildTower(config);
                OnCloseClick();
            };
            spawnedItems.Add(buildingItem);
        }
        public void OnTabTowerClick()
        {
            DisplayBuildingsByType(BuildingType.Tower);
            Audio_Manager.instance.play("sfx_ui_switch_tab");
        }

        public void OnTabResourceClick()
        {
            DisplayBuildingsByType(BuildingType.Resource);
            Audio_Manager.instance.play("sfx_ui_switch_tab");
        }

        public void OnTabBarrackClick()
        {
            DisplayBuildingsByType(BuildingType.Troop);
            Audio_Manager.instance.play("sfx_ui_switch_tab");
        }
        public void OnCloseClick()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, 0.3f);
        }
    }
}