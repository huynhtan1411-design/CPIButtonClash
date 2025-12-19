using System.Collections.Generic;
using UnityEngine;
using TemplateSystems;
using UISystems;
using ButtonClash.UI;

public class ProgressUIManager : MonoBehaviour
{
    [SerializeField] private UIProgressReward progressReward;
    
    public void UpdateProgressValue(int levelIndex, float progressValue)
    {
        if (progressReward != null)
        {
            progressReward.SetProgressAndCheckRewards(levelIndex, progressValue);
        }
    }

    
    public void ViewProgressRewardForLevel(int levelIndex)
    {
        if (progressReward != null)
        {
            progressReward.LoadProgressRewardForLevel(levelIndex);
        }
    }

    private void SetProgressForClearType(int levelIndex, ClearType clearType)
    {
        if (progressReward != null)
        {
            progressReward.SetProgressForClearType(levelIndex, clearType);
        }
    }
    
    public void SetPartialClear(int levelIndex)
    {
        SetProgressForClearType(levelIndex, ClearType.PartialClear);
    }
    
    public void SetFullClear(int levelIndex)
    {
        SetProgressForClearType(levelIndex, ClearType.FullClear);
    }
    
    public void SetPerfectClear(int levelIndex)
    {
        SetProgressForClearType(levelIndex, ClearType.PerfectClear);
    }
}