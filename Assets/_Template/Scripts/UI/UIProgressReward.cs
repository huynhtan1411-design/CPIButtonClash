using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ButtonClash.UI;
using TemplateSystems;
using UISystems;
using System.Linq;
using DG.Tweening;

public class UIProgressReward : MonoBehaviour
{
    private const int MILESTONE_COUNT = 3;

    public enum ChestState
    {
        Locked,
        Available,
        Collected
    }



    [System.Serializable]
    public class MilestoneChest
    {
        public RectTransform rectTransform;
        public Button chestButton;
        public Image icon;
        public ParticleSystem particleSystem;
        [Tooltip("Sprites: 0=Locked, 1=Available, 2=Collected")]
        public Sprite[] chestSprites = new Sprite[3];
        [HideInInspector]
        public ChestState state = ChestState.Locked;
        public ClearType clearType;
    }

    [Header("UI References")]
    [SerializeField] private MenuLevel menuLevel;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private MilestoneChest[] milestoneChests = new MilestoneChest[MILESTONE_COUNT];
    [SerializeField] private UIRewardInfo uiRewardInfo;
    [SerializeField] private TextMeshProUGUI txtLongestSurvival;



    [Header("Config")]
    [SerializeField] private ProgressRewardConfig progressRewardConfig;
    
    private bool[] milestoneCollected = new bool[MILESTONE_COUNT];
    private int selectedMilestoneIndex = -1;
    private float currentProgress = 0f;
    
    private int currentLevelIndex;

    private void Start()
    {
        InitializeMilestoneTypes();
        SetupChestButtons();
        //LoadProgressRewardConfig(DataManager.Instance.GetLevel());
    }
    
    private void InitializeMilestoneTypes()
    {
        milestoneChests[0].clearType = ClearType.PartialClear;
        milestoneChests[1].clearType = ClearType.FullClear;
        milestoneChests[2].clearType = ClearType.PerfectClear;
    }
    
    private void SetupChestButtons()
    {
        for (int i = 0; i < milestoneChests.Length; i++)
        {
            int index = i;
            if (milestoneChests[i].chestButton != null)
                milestoneChests[i].chestButton.onClick.AddListener(() => OnChestClicked(index));
        }
    }
    
    private void LoadProgressRewardConfig(int level)
    {
        currentLevelIndex = level;
        LoadCollectedState(level);
        
        float savedProgress = DataManager.Instance.GetProgressValue(level);
        UpdateProgressUI(savedProgress, level);
    }
    
    public void LoadProgressRewardForLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        LoadCollectedState(levelIndex);
        
        float savedProgress = DataManager.Instance.GetProgressValue(levelIndex);
        UpdateProgressUIWithoutSaving(savedProgress);
    }
    
    private void UpdateProgressUIWithoutSaving(float progress)
    {
        currentProgress = progress;
        
        if (progressSlider != null)
            progressSlider.value = progress;
        
        UpdateChestStates(progress);
    }
    
    private void UpdateProgressUI(float progress, int level)
    {
        currentProgress = progress;
        currentLevelIndex = level;
        
        DataManager.Instance.SaveProgressValue(level, progress);
        
        if (progressSlider != null)
            progressSlider.value = progress;
        
        UpdateChestStates(progress);
    }
    
    private void UpdateChestStates(float progress)
    {
        ClearType progressClearType = GetClearTypeFromProgress(progress);
        
        for (int i = 0; i < milestoneChests.Length; i++)
        {
            ChestState newState = DetermineChestState(progressClearType, i);
            UpdateChestState(i, newState);
        }
    }
    
    private ChestState DetermineChestState(ClearType progressClearType, int index)
    {
        if ((int)progressClearType >= (int)milestoneChests[index].clearType)
        {
            return milestoneCollected[index] ? ChestState.Collected : ChestState.Available;
        }
        
        return ChestState.Locked;
    }
    
    private ClearType GetClearTypeFromProgress(float progress)
    {
        if (progress >= 1.0f)
            return ClearType.PerfectClear;
        else if (progress >= 0.5f)
            return ClearType.FullClear;
        else if (progress >= 0f)
            return ClearType.PartialClear;
        else return ClearType.None;
    }
    
    private void SaveCollectedState(int level)
    {
        DataManager.Instance.SaveAllProgressRewardStates(level, milestoneCollected);
    }
    
    private void LoadCollectedState(int level)
    {
        bool[] savedStates = DataManager.Instance.GetAllProgressRewardStates(level, MILESTONE_COUNT);
        for (int i = 0; i < milestoneCollected.Length; i++)
        {
            milestoneCollected[i] = savedStates[i];
        }
    }
    
    private void CheckAndCollectAvailableMilestones(int level)
    {
        SaveCollectedState(level);
    }

    private void CollectMilestoneReward(int milestoneIndex, List<ItemInfo> itemInfos)
    {
        foreach (ItemInfo reward in itemInfos)
        {
            ProcessRewardItem(reward);
        }

        milestoneCollected[milestoneIndex] = true;
        UpdateChestState(milestoneIndex, ChestState.Collected);
    }
    
    private void ProcessRewardItem(ItemInfo reward)
    {
        if (reward.ItemType == "Currency" && reward.Id == "2")
        {
            DataManager.AddCoins((int)reward.Quantity);
        }
        else if (reward.ItemType == "Equipment" && !string.IsNullOrEmpty(reward.Id))
        {
            AddEquipmentReward(reward.Id);
        }
    }
    
    private void AddEquipmentReward(string equipmentId)
    {
        var equipData = DataManager.Instance.GetEquipmentInfo(equipmentId);
        if (equipData != null)
        {
            var equipmentList = new List<EquipmentInfData> { equipData };
            DataManager.Instance.AddEquipmentIntoBag(equipmentList);
        }
    }

    private void UpdateChestState(int chestIndex, ChestState newState)
    {
        if (chestIndex < 0 || chestIndex >= milestoneChests.Length)
            return;
            
        MilestoneChest chest = milestoneChests[chestIndex];
        chest.state = newState;
        
        UpdateChestVisuals(chest, newState);
        UpdateLongestTimeText();
    }
    private void UpdateLongestTimeText()
    {
        int longestTime = DataManager.Instance.GetLongestTimeSurvival(currentLevelIndex);
        int minutes = Mathf.FloorToInt(longestTime / 60);
        int seconds = Mathf.FloorToInt(longestTime % 60);

        string value = string.Format("{0:00}:{1:00}", minutes, seconds);
        txtLongestSurvival.text = string.Format("Longest Time Survived: {0}", value);
    }
    private void UpdateChestVisuals(MilestoneChest chest, ChestState newState)
    {
        uiRewardInfo.Hide();
        if (chest.icon != null && chest.chestSprites.Length >= 3)
        {
            chest.icon.sprite = chest.chestSprites[(int)newState];
        }
        
        //if (chest.chestButton != null)
        //{
        //    chest.chestButton.interactable = (newState == ChestState.Available);
        //}
        if (newState == ChestState.Available)
        {
            chest.particleSystem.gameObject.SetActive(true);
            
            if (chest.icon != null)
            {
                DOTween.Kill(chest.icon);
                chest.icon.transform.localRotation = Quaternion.identity;
                chest.icon.transform.DOShakeRotation(0.5f, new Vector3(0f, 0f, 7f), 10, 70, false).SetEase(Ease.OutBack)
                    .SetLoops(-1, LoopType.Restart);
            }
        }
        else
        {
            chest.particleSystem.gameObject.SetActive(false);
            
            if (chest.icon != null)
            {
                DOTween.Kill(chest.icon.transform);
                chest.icon.transform.localRotation = Quaternion.identity;
            }
        }
    }
    
    private void OnChestClicked(int milestoneIndex)
    {
        if (!IsValidChestSelection(milestoneIndex))
        {
            var infoReward = progressRewardConfig.GetRewardInfo(currentLevelIndex, milestoneChests[milestoneIndex].clearType);
            uiRewardInfo.Setup(milestoneChests[milestoneIndex].rectTransform, infoReward);
            return;
        }

        selectedMilestoneIndex = milestoneIndex;
        ShowRewardPopup(milestoneIndex, currentLevelIndex);
    }
    
    private bool IsValidChestSelection(int milestoneIndex)
    {
        return milestoneIndex >= 0 && 
               milestoneIndex < milestoneChests.Length && 
               milestoneChests[milestoneIndex].state == ChestState.Available;
    }
    
    private void ShowRewardPopup(int milestoneIndex, int level)
    {
        var levelReward = progressRewardConfig.GetRewardsAtProgress(level, milestoneChests[milestoneIndex].clearType);
        menuLevel.ShowReward(levelReward);
        menuLevel.OnRewardClosed = () => OnCollectReward(level, levelReward);
    }
    
    private void OnCollectReward(int level, List<ItemInfo> itemInfos)
    {
        if (selectedMilestoneIndex < 0 || selectedMilestoneIndex >= milestoneChests.Length)
            return;
            
        CollectMilestoneReward(selectedMilestoneIndex, itemInfos);
        SaveCollectedState(level);
        
        Audio_Manager.instance.play("Reward_Collected");
        OnCloseRewardPopup();
    }
    
    private void OnCloseRewardPopup()
    {
        selectedMilestoneIndex = -1;
    }
    
    public void SetProgressForClearType(int level, ClearType clearType)
    {
        float progress = ConvertClearTypeToProgress(clearType);
        SetProgressAndCheckRewards(level, progress);
    }
    
    private float ConvertClearTypeToProgress(ClearType clearType)
    {
        switch (clearType)
        {
            case ClearType.FullClear:
                return 0.5f;
            case ClearType.PerfectClear:
                return 1f;
            case ClearType.PartialClear:
            default:
                return 0f;
        }
    }
    
    public void SetProgressAndCheckRewards(int level, float progress)
    {
        UpdateProgressUI(progress, level);
        CheckAndCollectAvailableMilestones(level);
    }
} 