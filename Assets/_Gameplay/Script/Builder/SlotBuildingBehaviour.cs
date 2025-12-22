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
    [SerializeField] private bool useGoldFlyDrain = true;
    [SerializeField] private bool useCostAsFlyCount = true;


    [SerializeField] private int levelInit = 1;
    [SerializeField] private float timeAnimation = 1f;

    private GameObject buildingObj;
    private BaseBuildingBehaviour buildingBehaviour;
    private bool isBuilding = false;
    private bool isEmpty = true;
    private int pendingCost;
    private int coinsArrived;
    private int targetCost;
    private bool waitForCoins;
    public BuildingType SlotType => slotBuildingType;
    public BaseBuildingBehaviour BuildingBehaviour => buildingBehaviour;
    public int LevelUnlockCondition => levelUnlockCondition;
    public BuildingData BuildingData => data;
    public int LevelInit => levelInit;

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
    private bool HasEnoughGold(int cost)
    {
        if (WD.GameManager.Instance == null) return false;
        if (WD.GameManager.Instance.PlayerGold < cost)
        {
            Debug.LogWarning($"Not enough gold. Need {cost}, have {WD.GameManager.Instance.PlayerGold}", this);
            return false;
        }
        return true;
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
        Debug.Log($"[SlotBuilding] BuildTower called. isBuilding={isBuilding} isEmpty={isEmpty} slotType={slotBuildingType} currentGold={(WD.GameManager.Instance != null ? WD.GameManager.Instance.PlayerGold : -1)}", this);
        if (isBuilding) return;
        if (!IsUnlockedForBuild())
        {
            Debug.LogWarning($"Slot locked. Required zone level {levelUnlockCondition}, building unlock {data.LevelIndexUnlock}", this);
            return;
        }
        if (IsEmpty())
        {
            int cost = data.GetCostUpgrade(1);
            Debug.Log($"[SlotBuilding] Building new {data.name} cost={cost}", this);
            if (!HasEnoughGold(cost))
                return;
            bool drainByCoins = useGoldFlyDrain && useCostAsFlyCount && resourceFlyEffect != null && WDPlayerController.Instance != null;
            Debug.Log($"[SlotBuilding] drainByCoins={drainByCoins} useGoldFlyDrain={useGoldFlyDrain} useCostAsFlyCount={useCostAsFlyCount} resourceFlyEffectNull={resourceFlyEffect==null}", this);
            pendingCost = drainByCoins ? cost : 0;
            coinsArrived = 0;
            targetCost = drainByCoins ? cost : 0;
            waitForCoins = drainByCoins;

            isEmpty = false;

            Transform resourceSpawnPoint = WDPlayerController.Instance.transform;
            // Play resource fly effect before building animation
            if (resourceFlyEffect != null && resourceSpawnPoint != null)
            {
                Vector3 targetPos = transform.position; // + Vector3.up * 0.5f; // Slightly above the slot
                int flyCount = useCostAsFlyCount ? cost : resourceFlyEffect.ResourceCount;
                resourceFlyEffect.PlayResourceFlyEffect(resourceSpawnPoint, targetPos, flyCount, drainByCoins ? OnGoldFlyArrived : null);
            }
            else
            {
                Debug.Log($"[SlotBuilding] No resourceFlyEffect. Deduct gold immediately {cost}", this);
                waitForCoins = false;
                if (WD.GameManager.Instance != null)
                    WD.GameManager.Instance.AddGold(-cost);
            }
            if (!drainByCoins && WD.GameManager.Instance != null && resourceFlyEffect != null)
            {
                Debug.Log($"[SlotBuilding] Not draining per coin, deduct gold immediately {cost}", this);
                WD.GameManager.Instance.AddGold(-cost);
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
                if (buildingBehaviour != null && buildingBehaviour.IsMaxLevel())
                {
                    model.SetActive(false);
                }
                Audio_Manager.instance.play("sfx_summon_hero");
                onBuildingComplete?.Invoke();

                DOVirtual.DelayedCall(0.25f, delegate
                {
                    onBuildingCompleteDelay?.Invoke();
                });

                // Drain gold per coin arrival
                coinsArrived = 0;
                pendingCost = 0;
            });
        }
        else if (buildingBehaviour != null && buildingBehaviour.CanUpgrade())
        {
            int cost = data.GetCostUpgrade(buildingBehaviour.CurrentLevel + 1);
            Debug.Log($"[SlotBuilding] Upgrade {data.name} from lv {buildingBehaviour.CurrentLevel} cost={cost}", this);
            if (!HasEnoughGold(cost))
                return;
            bool drainByCoins = useGoldFlyDrain && useCostAsFlyCount && resourceFlyEffect != null && WDPlayerController.Instance != null;
            Debug.Log($"[SlotBuilding] drainByCoins={drainByCoins} useGoldFlyDrain={useGoldFlyDrain} useCostAsFlyCount={useCostAsFlyCount} resourceFlyEffectNull={resourceFlyEffect==null}", this);
            pendingCost = drainByCoins ? cost : 0;
            coinsArrived = 0;
            targetCost = drainByCoins ? cost : 0;
            waitForCoins = drainByCoins;

            Transform resourceSpawnPoint = WDPlayerController.Instance.transform;
            // Play resource fly effect before building animation
            if (resourceFlyEffect != null && resourceSpawnPoint != null)
            {
                Vector3 targetPos = transform.position; // + Vector3.up * 0.5f; // Slightly above the slot
                int flyCount = useCostAsFlyCount ? cost : resourceFlyEffect.ResourceCount;
                resourceFlyEffect.PlayResourceFlyEffect(resourceSpawnPoint, targetPos, flyCount, drainByCoins ? OnGoldFlyArrived : null);
            }
            else
            {
                Debug.Log($"[SlotBuilding] No resourceFlyEffect. Deduct gold immediately {cost}", this);
                waitForCoins = false;
                if (WD.GameManager.Instance != null)
                    WD.GameManager.Instance.AddGold(-cost);
            }
            if (!drainByCoins && WD.GameManager.Instance != null && resourceFlyEffect != null)
            {
                Debug.Log($"[SlotBuilding] Not draining per coin, deduct gold immediately {cost}", this);
                WD.GameManager.Instance.AddGold(-cost);
            }
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
        Debug.Log($"[SlotBuilding] Start build routine waitForCoins={waitForCoins} targetCost={targetCost} timeAnimation={timeAnimation}", this);
        isBuilding = true;
        buildingProgressSlider.gameObject.SetActive(true);
        if (buildingProgressSlider != null)
        {
            buildingProgressSlider.gameObject.SetActive(true);
            buildingProgressSlider.fillAmount = 0;
        }

        buildingEffect.Play();
        Audio_Manager.instance.play("sfx_ui_building");

        if (waitForCoins && targetCost > 0)
        {
            while (coinsArrived < targetCost)
            {
                if (buildingProgressSlider != null && targetCost > 0)
                {
                    buildingProgressSlider.fillAmount = Mathf.Clamp01((float)coinsArrived / targetCost);
                }
                yield return null;
            }
        }
        else
        {
            float elapsedTime = 0;
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
        }
        
        if (buildingProgressSlider != null)
        {
            buildingProgressSlider.gameObject.SetActive(false);
        }

        onComplete?.Invoke();
        isBuilding = false;
    }

    private void OnGoldFlyArrived(int amount)
    {
        if (!useGoldFlyDrain) return;
        if (WD.GameManager.Instance == null) return;

        if (pendingCost > 0)
        {
            Debug.Log($"[SlotBuilding] OnGoldFlyArrived amount={amount} pendingCost(before)={pendingCost}", this);
            WD.GameManager.Instance.AddGold(-amount);
            pendingCost -= amount;
            coinsArrived += amount;
            if (pendingCost < 0)
                pendingCost = 0;
        }
    }

    private bool IsUnlockedForBuild()
    {
        if (slotBuildingType == BuildingType.Base)
            return true;
        int currentZone = SafeZoneController.Instance != null ? SafeZoneController.Instance.CurrentZoneLevel : 0;
        int requiredZone = Mathf.Max(levelUnlockCondition, data != null ? data.LevelIndexUnlock : 0);
        var lvlData = data != null ? data.GetLevelData(levelInit) : null;
        if (lvlData != null)
            requiredZone = Mathf.Max(requiredZone, lvlData.levelUnlockCondition);
        Debug.Log($"[SlotBuilding] IsUnlockedForBuild currentZone={currentZone} requiredZone={requiredZone} slotType={slotBuildingType}", this);
        return currentZone >= requiredZone;
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
            if (buildingBehaviour != null && buildingBehaviour.IsMaxLevel())
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
