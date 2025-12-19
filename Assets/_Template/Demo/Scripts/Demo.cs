using System.Collections.Generic;
using TemplateSystems;
using UISystems;
using UnityEngine;

public class Demo : MonoBehaviour
{
    private int _currentLevelIndex = 0;
    private void Start()
    {
        UIManager.onLevelCompleteSet += IncreaseLevelIndex;
        UIManager.onNextLevelButtonPressed += SpawnLevel;
        UIManager.onRetryButtonPressed += RetryLevel;
        SpawnLevel();
    }

    private void OnDestroy()
    {
        UIManager.onLevelCompleteSet -= IncreaseLevelIndex;
        UIManager.onNextLevelButtonPressed -= SpawnLevel;
        UIManager.onRetryButtonPressed -= RetryLevel;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.W))
        {
            //_uiGame.sortingOrder = -1;
            ShowUIWin();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //_uiGame.sortingOrder = -1;
            ShowUIGameOver();
        }

#endif
    }
    private void RetryLevel()
    {
        Debug.Log("RetryLevel");
    }
    private void SpawnLevel()
    {
        _currentLevelIndex = PlayerPrefs.GetInt(TYPELEVEL.LEVEL.ToString(), 0);
        int level = _currentLevelIndex + 1;
        Debug.Log("LoadLevel " + level);
        LoadLevel(_currentLevelIndex);
    }

    private void LoadLevel(int currentLevelIndex)
    {

    }

    public void ShowUIWin()
    {
        UIManager.setLevelCompleteDelegate?.Invoke();
        var lstEquip = GetLevelRewards();
        var lstHeroCard = DataManager.Instance.RewardHeroCards();
        DataManager.Instance.UnlockHero();
        UIManager.instance.UILevelWin.SetupReward(10, lstEquip, lstHeroCard);
    }
    private static List<EquipmentInfData> GetLevelRewards()
    {
        int itemCount = Utils.RandomByWeight(new float[] { 75f, 25f }) == 1 ? 2 : 1;
        return EquipmentRandomizer.GetRandomEquipmentsByRarity(
            DataManager.Instance.EquipmentInfoData.Data,
            EquipmentRandomizer.CreateDefaultRates(),
            itemCount
        );
    }
    public void ShowUIGameOver()
    {
        UIManager.setGameoverDelegate?.Invoke();
    }

    private void IncreaseLevelIndex(int starsCount)
    {
        AdvanceToNextLevel();
        UIManager.AddCoins(10 * _currentLevelIndex);
    }

    public void AdvanceToNextLevel()
    {
        _currentLevelIndex++;
        Debug.Log("AdvanceToNextLevel " + _currentLevelIndex);
        PlayerPrefs.SetInt(TYPELEVEL.LEVEL.ToString(), _currentLevelIndex);
    }
}
