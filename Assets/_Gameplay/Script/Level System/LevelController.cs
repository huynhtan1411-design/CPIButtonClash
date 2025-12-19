using CLHoma.Combat;
using System.Collections.Generic;
using TemplateSystems;
using UnityEngine;
using UISystems;
namespace CLHoma.LevelSystem
{
    public static class LevelController
    {
        public static LevelsDatabase levelsDatabase;

        private static GameObject levelGameObject;

        private static LevelData currentChapterData;
        public static LevelData CurrentChapterData => currentChapterData;

        private static int currentChapterIndex;
        public static void Initialise()
        {
            levelsDatabase.Initialise();
            levelGameObject = new GameObject("[LEVEL]");
            levelGameObject.transform.ResetGlobal();
            ActiveRoom.Initialise(levelGameObject);

        }
        private static void LoadChapter()
        {
            LevelData chapterData = levelsDatabase.GetLevel(currentChapterIndex);
            currentChapterData = chapterData;
            ActiveRoom.Setup();
            ActiveRoom.SetLevelData(chapterData, currentChapterIndex);
            ActiveRoom.ClearEnemies();
            EnvironmentTextureController.Instance.ChangeTextureByChapter(currentChapterIndex);
        }
        private static void LoadCurrentLevel()
        {
            LoadChapter();

            StartSpawningEnemies();
        }

        public static void StartSpawningEnemies()
        {
            RoundData[] rounds = currentChapterData.RoundDatas;
            _ = ActiveRoom.StartSpawningEnemies(rounds);
        }


        public static void StartGameplay(int levelIndex)
        {
            currentChapterIndex = levelIndex;
            PlayerController.Instance.Reload();
            SkillManager.Instance.Reload();
            LoadCurrentLevel();
            UIManager.instance.UIGameplayCtr.SetupTimer(currentChapterData.TotalLevelTime / 1000);
        }


        public static string GetCurrentAreaText()
        {
            return string.Format("Chapter {0}", currentChapterIndex + 1);
        }
        public static void OnEnemyKilled(BaseEnemyBehavior enemy)
        {
            foreach (var character in PlayerController.Instance.Characters)
            {
                character.EnemyDetector.OnEnemyDied(enemy);
            }

           // PlayerController.StatsManager.AddExperienceOnKillEnemy(enemy.Tier);
            ActiveRoom.CheckWinCondition(OnWinLevel);
        }

        public static void OnFailLevel()
        {
            ActiveRoom.CancelSpawningEnemies();

            UIManager.instance.UILevelLose.SetInfo(GetCurrentAreaText(), currentChapterData.TotalLevelTime / 1000);
            UIManager.instance.UILevelLose.SetupReward(ActiveRoom.GoldCollect, null);
            DataManager.AddCoins(ActiveRoom.GoldCollect);
            UpdateProgressReward(ActiveRoom.ProgressPercentage, false);
            UpdateLongestSurvival();
        }


        private static void OnWinLevel()
        {
            GameManager.Instance.CompleteLevel();

            var lstEquip = GetLevelRewards();
            var lstHeroCard = DataManager.Instance.RewardHeroCards();
            DataManager.Instance.UnlockHero();
            UIManager.instance.UILevelWin.SetInfo(GetCurrentAreaText(), currentChapterData.TotalLevelTime / 1000);
            UIManager.instance.UILevelWin.SetupReward(ActiveRoom.GoldCollect, lstEquip, lstHeroCard);

            DataManager.Instance.AddEquipmentIntoBag(lstEquip);
            DataManager.AddCoins(ActiveRoom.GoldCollect);
            UpdateProgressReward(ActiveRoom.ProgressPercentage, true);
            UpdateLongestSurvival();
            OnNextLevel();
        }

        private static void OnNextLevel()
        {
            int lastLevelIndex = DataManager.Instance.GetLevel();
            if (currentChapterIndex < lastLevelIndex)
            {
                return;
            }
            lastLevelIndex++;
            DataManager.Instance.SaveLevel(lastLevelIndex);
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

        private static void UpdateProgressReward(float progress, bool hasComplete)
        {
            if (progress < 0.5f)
                return;
            
            float progressValue;
            
            if (progress >= 1.0f && hasComplete)
            {
                progressValue = PlayerController.StatsManager.IsFullHealth ? 1.0f : 0.5f;
            }
            else
            {
                progressValue = 0f;
            }

            float currentProgress = DataManager.Instance.GetProgressValue(currentChapterIndex);
            if (currentProgress > progressValue)
                return;
            DataManager.Instance.SaveProgressValue(currentChapterIndex, progressValue);
        }
        private static void UpdateLongestSurvival()
        {
            int currentTime = UIManager.instance.UIGameplayCtr.GetSurvivalTimeInt();
            DataManager.Instance.SaveLongestTimeSurvival(currentChapterIndex, currentTime);
        }
    }
}