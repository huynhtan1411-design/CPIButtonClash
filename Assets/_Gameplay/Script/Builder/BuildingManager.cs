using System.Collections.Generic;
using UnityEngine;
using CLHoma.Combat;
using CLHoma;
using WD;
using UISystems;
using static UnityEditor.Progress;
public class BuildingManager : MonoSingleton<BuildingManager>
{
    public static event System.Action<int, int> OnResourceUpdated;
    [SerializeField] private List<SlotBuildingBehaviour> allBuildingSlots = new List<SlotBuildingBehaviour>();
    [SerializeField] private SlotBuildingBehaviour slotMain = null;
    [SerializeField] private GameObject firehall = null;


    [Space(10)]
    [Header("Building Group I")]
    [SerializeField] private List<SlotBuildingBehaviour> buildingGroupI;
    [Header("Building Group I")]
    [SerializeField] private List<SlotBuildingBehaviour> buildingGroupII;

    private int currentResources = 0;

    public List<GameObject> destroyEffectObjects = new List<GameObject>();
    public SlotBuildingBehaviour CurrentActiceSlotInteractive;
    public BaseBuildingBehaviour HallTowerBuilding;
    private void Start()
    {
        if (allBuildingSlots.Count == 0)
        {
            allBuildingSlots.AddRange(FindObjectsOfType<SlotBuildingBehaviour>());
        }
        
        UpdateBuildingSlotsVisibility();
        SetUpSilver();

        WD.GameManager.Instance.OnCombatPhaseStart.AddListener(UpdateBuildingSlotsVisibility);
        WD.GameManager.Instance.OnBuildPhaseStart.AddListener(UpdateBuildingSlotsVisibility);
        UIManager.onMenuSet += ResetForMainMenu;
        UIManager.onGameSet += ResetForGameplay;
        if (WD.GameManager.Instance != null)
            WD.GameManager.Instance.OnPlayerGoldChanged += SyncGold;
    }
    private void OnDestroy()
    {
        WD.GameManager.Instance.OnCombatPhaseStart.RemoveListener(UpdateBuildingSlotsVisibility);
        WD.GameManager.Instance.OnBuildPhaseStart.RemoveListener(UpdateBuildingSlotsVisibility);
        UIManager.onMenuSet -= ResetForMainMenu;
        UIManager.onGameSet -= ResetForGameplay;
        if (WD.GameManager.Instance != null)
            WD.GameManager.Instance.OnPlayerGoldChanged -= SyncGold;
    }

    private void ResetForMainMenu()
    {
        ResetAllSlots();
        //firehall.gameObject.SetActive(true);
    }
    private void ResetForGameplay()
    {
        //firehall.gameObject.SetActive(false);
    }

    public void SetUpSilver()
    {
        currentResources = WD.GameManager.Instance.GameConfig.silverStart;
        if (WD.GameManager.Instance != null)
            WD.GameManager.Instance.SetGold(currentResources);
        Debug.Log($"[BuildingManager] SetUpSilver -> currentResources={currentResources}");
        OnResourceUpdated?.Invoke(currentResources, 0);
    }

    public bool CheckSlotMain()
    {
        return !slotMain.IsEmpty();
    }
    public bool isUpgrade = false;
    private void UpdateBuildingSlotsVisibility()
    {
        int currentZoneLevel = isUpgrade? 3: 2;

        foreach (var slot in allBuildingSlots)
        {

            if (WD.GameManager.Instance.CurrentPhase == GamePhase.BuildPhase)
            {
                bool shouldBeVisible = CheckSlotUnlockCondition(slot, currentZoneLevel);
                slot.ToggleRendererSlot(shouldBeVisible);
            }
            else if (WD.GameManager.Instance.CurrentPhase == GamePhase.CombatPhase)
            {
                slot.ToggleRendererSlot(false);
            }
        }
        DestroyAllTempObject();
    }
    private void DestroyAllTempObject()
    {
        foreach (var obj in destroyEffectObjects)
        {
            Destroy(obj);
        }
        destroyEffectObjects.Clear();
    }

    private bool CheckSlotUnlockCondition(SlotBuildingBehaviour slot, int currentZoneLevel)
    {
        if (slot.SlotType == BuildingType.Base)
        {
            return true;
        }

        int required = slot.LevelUnlockCondition;
        if (slot.BuildingData != null)
        {
            required = Mathf.Max(required, slot.BuildingData.LevelIndexUnlock);
            var lvlData = slot.BuildingData.GetLevelData(slot.LevelInit);
            if (lvlData != null)
                required = Mathf.Max(required, lvlData.levelUnlockCondition);
        }
        return currentZoneLevel >= required;
    }

    public void OnZoneLevelChanged()
    {
        UpdateBuildingSlotsVisibility();
    }

    public void AddResources(int amount)
    {
        Debug.Log($"[BuildingManager] AddResources amount={amount}");
        if (WD.GameManager.Instance != null)
            WD.GameManager.Instance.AddGold(amount);
        SyncGold(WD.GameManager.Instance != null ? WD.GameManager.Instance.PlayerGold : currentResources);
    }
    public bool CanAffordBuilding(int cost)
    {
        return currentResources >= cost;
    }
    public void SpendResources(int amount)
    {
        if (CanAffordBuilding(amount))
        {
            currentResources -= amount;
            OnResourceUpdated?.Invoke(currentResources, -amount);
        }
    }

    private void SyncGold(int gold)
    {
        Debug.Log($"[BuildingManager] SyncGold {currentResources} -> {gold}");
        currentResources = gold;
        OnResourceUpdated?.Invoke(currentResources, 0);
    }

    public void ResetAllSlots()
    {
        // foreach (var slot in allBuildingSlots)
        // {
        //     if (slot != null)
        //     {
        //         slot.ResetSlot();
        //     }
        // }
        // if (slotMain != null)
        //     slotMain.ResetSlot();
        SafeZoneController.Instance.ResetSafeZone();
    }

    public bool IsFullHealth()
    {
        if (HallTowerBuilding == null) return false;
        return HallTowerBuilding.BaseDamageableEntity.IsFullHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            foreach (var item in allBuildingSlots)
            {
                item.BuildTower();
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            foreach (var item2 in buildingGroupI)
            {
                item2.BuildTower();
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            foreach (var item3 in buildingGroupII)
            {
                item3.BuildTower();
            }
        }
    }
} 
