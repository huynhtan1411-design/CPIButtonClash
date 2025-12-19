using WD;
using System.Collections;
using System.Collections.Generic;
using UISystems;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using CLHoma.Combat;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.Events;
using DG.Tweening;
public enum BuildingType
{
    Base,
    Tower,
    Resource,
    Troop,
    Wall
}
public class SlotBuildingBehaviour : MonoBehaviour
{

    [SerializeField] UnityEvent onBuildingComplete;
    [SerializeField] UnityEvent onBuildingCompleteDelay;

    [Space(10)]
    [SerializeField] BuildingType slotBuildingType;
    [SerializeField] private Transform parentBuilding;
    [SerializeField] private GameObject model;
    [SerializeField] private ParticleSystem disapearEffect;
    [SerializeField] private ParticleSystem buildingEffect;
    [SerializeField] private int levelUnlockCondition;
    [SerializeField] private Image buildingProgressSlider;
    [SerializeField] private BuildingData data;
    [SerializeField] private ResourceFlyEffect resourceFlyEffect;


    [SerializeField] private int levelInit = 1;
    [SerializeField] private float timeAnimation = 1f;

    private GameObject buildingObj;
    private BaseBuildingBehaviour buildingBehaviour;
    private bool isBuilding = false;
    private bool isEmpty = true;
    public BuildingType SlotType => slotBuildingType;
    public BaseBuildingBehaviour BuildingBehaviour => buildingBehaviour;
    public int LevelUnlockCondition => levelUnlockCondition;

    private void Start()
    {
        if (SafeZoneController.Instance != null)
        {
            SafeZoneController.Instance.OnZoneLevelChanged += OnZoneLevelChanged;
        }
        buildingProgressSlider.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (SafeZoneController.Instance != null)
        {
            SafeZoneController.Instance.OnZoneLevelChanged -= OnZoneLevelChanged;
        }
    }

    private void OnZoneLevelChanged()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.OnZoneLevelChanged();
        }
    }
    public bool IsEmpty()
    {
        return isEmpty;
    }
    private void SpawnBuilding()
    {
        var buildingPrefab = data.Prefab;
        if (buildingPrefab == null) return;
        disapearEffect.Play();
        buildingObj = Instantiate(buildingPrefab);
        buildingObj.transform.SetParent(parentBuilding);
        buildingObj.transform.localRotation = Quaternion.identity;
        buildingObj.transform.localPosition = Vector3.zero;
        buildingObj.transform.localScale = Vector3.one;

        buildingBehaviour = buildingObj.GetComponent<BaseBuildingBehaviour>();
        buildingBehaviour.CurrentLevel = levelInit;
        buildingBehaviour.Initialize(data);
    }

    public void BuildTower()
    {
        if (isBuilding) return;
        if (IsEmpty())
        {
            isEmpty = false;
            //BuildingManager.Instance.SpendResources(data.GetCostUpgrade(1));

            Transform resourceSpawnPoint = WDPlayerController.Instance.transform;
            // Play resource fly effect before building animation
            if (resourceFlyEffect != null && resourceSpawnPoint != null)
            {
                Vector3 targetPos = transform.position; // + Vector3.up * 0.5f; // Slightly above the slot
                resourceFlyEffect.PlayResourceFlyEffect(resourceSpawnPoint, targetPos);
            }
            
            PlayBuildingAnimation(() => {
                SpawnBuilding();
                if (slotBuildingType == BuildingType.Base)
                {
                    GameManager.Instance.OpenFightButton();
                    SafeZoneController.Instance.UpgradeZoneLevel();
                }
                if (slotBuildingType == BuildingType.Wall)
                {
                    buildingBehaviour.SetModelWall(levelUnlockCondition);
                    buildingBehaviour.BuildingGraphics.AnimationDropOfWall();
                }
                Audio_Manager.instance.play("sfx_summon_hero");
                onBuildingComplete?.Invoke();

                DOVirtual.DelayedCall(0.25f, delegate
                {
                    onBuildingCompleteDelay?.Invoke();
                });
            });
        }
        else if (buildingBehaviour != null && buildingBehaviour.CanUpgrade())
        {

            Transform resourceSpawnPoint = WDPlayerController.Instance.transform;
            // Play resource fly effect before building animation
            if (resourceFlyEffect != null && resourceSpawnPoint != null)
            {
                Vector3 targetPos = transform.position; // + Vector3.up * 0.5f; // Slightly above the slot
                resourceFlyEffect.PlayResourceFlyEffect(resourceSpawnPoint, targetPos);
            }
            //BuildingManager.Instance.SpendResources(data.GetCostUpgrade(buildingBehaviour.CurrentLevel + 1));
            PlayBuildingAnimation(() => {
                buildingBehaviour.Upgrade();
                if (slotBuildingType == BuildingType.Base)
                    SafeZoneController.Instance.UpgradeZoneLevel();
                if (slotBuildingType == BuildingType.Wall)
                {
                    buildingBehaviour.SetModelWall(levelUnlockCondition);
                }

                if (buildingBehaviour.IsMaxLevel())
                {
                    model.SetActive(false);
                }

                Audio_Manager.instance.play("sfx_summon_hero");
                onBuildingComplete?.Invoke();

                DOVirtual.DelayedCall(0.25f, delegate
                {
                    onBuildingCompleteDelay?.Invoke();
                });
            });
        }
    }

    private void PlayBuildingAnimation(System.Action onComplete)
    {
        StartCoroutine(BuildingAnimationRoutine(onComplete));
    }

    private IEnumerator BuildingAnimationRoutine(System.Action onComplete)
    {
        isBuilding = true;
        buildingProgressSlider.gameObject.SetActive(true);
        if (buildingProgressSlider != null)
        {
            buildingProgressSlider.gameObject.SetActive(true);
            buildingProgressSlider.fillAmount = 0;
        }

        float elapsedTime = 0;
        buildingEffect.Play();
        Audio_Manager.instance.play("sfx_ui_building");

        while (elapsedTime < timeAnimation)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / timeAnimation;
            
            if (buildingProgressSlider != null)
            {
                buildingProgressSlider.fillAmount = progress;
            }
            
            yield return null;
        }
        
        if (buildingProgressSlider != null)
        {
            buildingProgressSlider.gameObject.SetActive(false);
        }

        onComplete?.Invoke();
        isBuilding = false;
    }

    public void ResetSlot()
    {
        if (!IsEmpty())
        {
            isEmpty = true;
            if (disapearEffect != null)
            {
                disapearEffect.Play();
            }
            
            if (buildingObj != null)
            {
                Destroy(buildingObj);
                buildingObj = null;
                buildingBehaviour = null;
            }

            if (model != null)
            {
                model.SetActive(true);
            }
        }
    }

    public void ToggleRendererSlot(bool value)
    {
        model.gameObject.SetActive(value);
    }

    public void BuildTowerWithOutAnimation()
    {
        if (isBuilding) return;
        if (IsEmpty())
        {
            isEmpty = false;
            SpawnBuilding();
            if (slotBuildingType == BuildingType.Base)
            {
                GameManager.Instance.OpenFightButton();
                SafeZoneController.Instance.UpgradeZoneLevel();
            }
            if (slotBuildingType == BuildingType.Wall)
            {
                buildingBehaviour.SetModelWall(levelUnlockCondition);
                buildingBehaviour.BuildingGraphics.AnimationDropOfWall();
            }
            Audio_Manager.instance.play("sfx_summon_hero");
            onBuildingComplete?.Invoke();

            DOVirtual.DelayedCall(0.25f, delegate
            {
                onBuildingCompleteDelay?.Invoke();
            });
        }
        else if (buildingBehaviour != null && buildingBehaviour.CanUpgrade())
        {

            buildingBehaviour.Upgrade();
            if (slotBuildingType == BuildingType.Base)
                SafeZoneController.Instance.UpgradeZoneLevel();
            if (slotBuildingType == BuildingType.Wall)
            {
                buildingBehaviour.SetModelWall(levelUnlockCondition);
            }

            if (buildingBehaviour.IsMaxLevel())
            {
                model.SetActive(false);
            }

            Audio_Manager.instance.play("sfx_summon_hero");
            onBuildingComplete?.Invoke();

            DOVirtual.DelayedCall(0.25f, delegate
            {
                onBuildingCompleteDelay?.Invoke();
            });
        }
    }

}