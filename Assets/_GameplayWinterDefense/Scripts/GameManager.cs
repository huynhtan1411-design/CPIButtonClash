using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using CLHoma;
using UISystems;
using UnityEngine.UI;
using CLHoma.LevelSystem;
using System.Linq;
using System;
using CLHoma.Combat;
using TemplateSystems;
using DG.Tweening;
using Cinemachine;

namespace WD
{
    public enum GamePhase
    {
        BuildPhase,    // Building and upgrading towers
        CombatPhase,   // Enemy waves attacking
        EndPhase       // Level end (win/lose)
    }
    public enum TargetType
    {
        Player,
        Tower,
        Base,
        Wall,
        HouseResource
    }
    public enum SpawnDirection
    {
        Top,
        Left,
        Right,
        Down,
        TopLeft,
        TopRight,
        DownLeft,
        DownRight,
        Random
    }
    [System.Serializable]
    public class LevelConfig
    {
        //public string levelName;              // Name of the level
        public List<WaveConfig> waves;       // List of waves for this level
        public int startingGold;              // Starting gold amount
        //public float buildTime;               // Maximum build time (if fixed)
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameConfig GameConfig => gameConfig;


        [Header("Levels")]
        [SerializeField] private LevelConfigSO levelConfigSO;    // Level configurations ScriptableObject
        private int currentLevelIndex = 0;

        [Header("References")]
        public WaveSpawner waveSpawner;        // Manages enemy spawns
        [Header("Game Config")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private WDPlayerController playerController;
        [SerializeField] private UpgradesController upgradesController;
        [SerializeField] private ParticlesController particlesController;
        [SerializeField] private FloatingTextController floatingTextController;
        [SerializeField] private Light light;
        [Header("Events")]
        public UnityEvent OnBuildPhaseStart;   // Fired when build phase begins
        public UnityEvent OnCombatPhaseStart;  // Fired when combat phase begins
        public UnityEvent OnLevelComplete;     // Fired on level victory
        public UnityEvent OnLevelFailed;       // Fired on level failure
        public UnityEvent OnWaveComplete;      // Fired when a wave is completed

        private static GameObject levelGameObject;
        private GamePhase currentPhase;
        private LevelConfig currentLevel;
        private int playerGold;
        private bool isGamePaused;
        private Button _fightBtn;
        private List<GameObject> activeEnemies = new List<GameObject>();
        public int RadiusArea = 10;

        [Header("Wave Settings")]
        [SerializeField] private int totalWaves = 4;
        [SerializeField] private float timeBetweenWaves = 5f;
        
        private int currentWave = 1;
        private bool isGameOver = false;

        public int CurrentWave => currentWave;
        public int TotalWaves => totalWaves;
        public GamePhase CurrentPhase => currentPhase;
        public event Action<int, int> OnWaveChanged; // (currentWave, totalWaves)
        public event Action OnGameOver;

        public static event Action<bool> OnGamePauseStateChanged;
        private UIGameCtr uIGameplayCtr = null;
        private WaveData waveData = null;
        [Header("Lighting Settings")]
        [SerializeField] private float dayLightIntensity = 1f;
        [SerializeField] private float nightLightIntensity = 0.3f;
        [SerializeField] private float lightTransitionDuration = 3f;
        [SerializeField] private Light directionalLight;

        public void RegisterEnemy(GameObject enemy)
        {
            if (enemy != null && !activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
            }
        }

        public void UnregisterEnemy(GameObject enemy)
        {
            if (enemy != null && activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                CheckWaveCompletion();
            }
        }

        private void CheckWaveCompletion()
        {
            if (activeEnemies.Count <= 0 && currentPhase == GamePhase.CombatPhase)
            {
                if (waveSpawner.IsCurrentWaveComplete())
                {
                    OnWaveComplete?.Invoke();
                    
                    if (waveSpawner.HasNextWave())
                    {
                        EnterBuildPhase();
                    }
                    else
                    {
                        HandleAllWavesComplete();
                    }
                }
            }
        }

        public int GetActiveEnemyCount()
        {
            return activeEnemies.Count;
        }

        public List<GameObject> GetActiveEnemies()
        {
            return new List<GameObject>(activeEnemies); // Return a copy to prevent external modification
        }

        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies.ToList()) // Use ToList() to avoid modification during iteration
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            activeEnemies.Clear();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {

                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            currentLevelIndex = DataManager.Instance.GetLevel();
            Debug.LogError("currentLevelIndex "+ currentLevelIndex);
            OnWaveComplete.AddListener(() => {
                BuildingManager.Instance.AddResources(waveData.SilverReward);
            });
            InitializeEventListeners();
            InitialiseControllers();
            UIManager.instance.OnStart();

            // Initialize light to day state
            if (directionalLight == null)
                directionalLight = light;
            SetDayLight(true);
        }
        private void OnDestroy()
        {
            UnsubscribeEvents();
            DOTween.Kill(directionalLight);
        }
        #region Event Handlers
        private void InitializeEventListeners()
        {
            UIManager.onMenuPauseSet += InitUIPause;
            UIManager.onPauseGameSet += PauseGame;
            UIManager.onResumeGame += ResumeGame;
            UIManager.onMenuSet += OnMenuSet;
            UIManager.onLoadGame += ReplayLevel;
            uIGameplayCtr = UIManager.instance.UIGameplayCtr;
            _fightBtn = uIGameplayCtr.FightButton;
            _fightBtn.onClick.AddListener(OnFightButtonClicked);
        }

        private void UnsubscribeEvents()
        {
            UIManager.onMenuPauseSet -= InitUIPause;
            UIManager.onPauseGameSet -= PauseGame;
            UIManager.onResumeGame -= ResumeGame;
            UIManager.onMenuSet -= OnMenuSet;
            UIManager.onLoadGame -= ReplayLevel;
        }

        private void InitialiseControllers()
        {
            upgradesController.Initialise();
            particlesController.Initialise();
            floatingTextController.Inititalise();
        }
        #endregion

        #region UI Management
        private void PauseGame()
        {
            isGamePaused = true;
            UIManager.instance.UIGameplayCtr.TogglePauseTimer(true);
            OnGamePauseStateChanged?.Invoke(true);
        }

        private void ResumeGame()
        {
            isGamePaused = false;
            UIManager.instance.UIGameplayCtr.TogglePauseTimer(false);
            OnGamePauseStateChanged?.Invoke(false);
        }

        public bool IsGamePaused()
        {
            return isGamePaused;
        }

        private void InitUIPause()
        {
            var selectedHeroes = new List<string>();
            var selectedSkills = new List<string>();

            if (SkillManager.Instance != null)
            {
                foreach (var skill in SkillManager.Instance.GetListSkillWithType(AbilityType.Special))
                    selectedSkills.Add(skill.skillData.Id);
            }

            UIManager.instance.UIPauseCtr.ShowUI(selectedHeroes, selectedSkills);
        }

        private void Update()
        {
            if (!isGamePaused)
            {
                UpdateGameLogic();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                StartCombatPhase();
            }
        }

        private void UpdateGameLogic()
        {
            // Move any game update logic here that needs to be paused
            CheckWaveCompletion();
            // Add other game logic that needs to be paused
        }

        // Helper method for other scripts to check pause state
        public static bool IsPaused()
        {
            return Instance != null && Instance.isGamePaused;
        }
        #endregion

        private void OnMenuSet()
        {
            playerController.RespawnPlayer();
        }

        private void LoadFisrtLevel()
        {
        }

        private void OnFightButtonClicked()
        {
            if (currentPhase == GamePhase.BuildPhase)
                StartCombatPhase();
        }

        public void StartLevel(int levelIndex)
        {
            Debug.LogError("StartLevel " + levelIndex);
            EnvironmentController.Instance.ToggleEnvironmentByLevelIndex(levelIndex);
            BuildingManager.Instance.ResetAllSlots();
            BuildingManager.Instance.SetUpSilver();
            playerController.RespawnPlayer();
            if (levelIndex < 0 || levelIndex >= levelConfigSO.levels.Count)
            {
                Debug.LogError("Level index is out of range!");
                return;
            }

            // Clear any remaining enemies from previous level
            ClearAllEnemies();

            currentLevelIndex = levelIndex;
            currentLevel = levelConfigSO.levels[levelIndex];
            playerGold = currentLevel.startingGold;

            waveSpawner.waves = currentLevel.waves;
            waveSpawner.ResetWaves();
            EnterBuildPhase();
        }

        private void EnterBuildPhase()
        {
            currentPhase = GamePhase.BuildPhase;
            if (BuildingManager.Instance.CheckSlotMain())
                OpenFightButton();
            else
                CloseFightButton();

            // Transition to day light
            SetDayLight();

            // Show spawn information for the next wave
            if (waveSpawner.HasNextWave())
            {
                WaveConfig nextWave = waveSpawner.GetNextWave();
                UIManager.instance.UIGameplayCtr.SpawnDirectionUI.ShowSpawnInfo(nextWave.enemyGroups);
            }
            
            OnBuildPhaseStart?.Invoke();

            Audio_Manager.instance.play("bgm_menu_B");
        }

        private IEnumerator BuildPhaseTimer(float duration)
        {
            float timer = duration;

            while (timer > 0 && currentPhase == GamePhase.BuildPhase)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            if (currentPhase == GamePhase.BuildPhase)
                StartCombatPhase();
        }

        public void StartCombatPhase()
        {
            currentPhase = GamePhase.CombatPhase;
            _fightBtn.gameObject.SetActive(false);
            // Clear spawn information UI
            UIManager.instance.UIGameplayCtr.SpawnDirectionUI.ClearSpawnInfo();
            waveSpawner.StartNextWave();
            OnCombatPhaseStart?.Invoke();
            // Transition to night light
            SetNightLight();

            Audio_Manager.instance.play("bgm_battle_start");

            // Play battle background music
            DOVirtual.DelayedCall(3f, delegate {
                Audio_Manager.instance.play("bgm_battle_B");
            });
        }

        private void HandleAllWavesComplete()
        {
            UpdateProgressReward(true);
            StartCoroutine(LevelSuccessRoutine());
        }

        private IEnumerator LevelSuccessRoutine()
        {
            currentPhase = GamePhase.EndPhase;
            yield return new WaitForSeconds(1f);
            OnLevelComplete?.Invoke();
            BuildingManager.Instance.ResetAllSlots();
            Debug.LogError("OnLevelComplete");
            currentLevelIndex++;
            DataManager.Instance.SaveLevel(currentLevelIndex);
            UIManager.instance.SetLevelComplete();
            var lstEquip = GetLevelRewards();
            var lstRewardCard = DataManager.Instance.RewardBuildingCards();
            Debug.LogError("lstRewardCard "+ lstRewardCard.Count);
            //DataManager.Instance.UnlockBuilding();
            UIManager.instance.UILevelWin.SetInfo(GetCurrentAreaText());
            UIManager.instance.UILevelWin.SetupRewardCard(ActiveRoom.GoldCollect, lstEquip, lstRewardCard);
        }

        public void HandleLevelFailed()
        {
            if (currentPhase != GamePhase.CombatPhase) return;
            BuildingManager.Instance.ResetAllSlots();
            currentPhase = GamePhase.EndPhase;
            OnLevelFailed?.Invoke();
            UIManager.instance.SetGameover();
            UpdateProgressReward(false);
        }
        public string GetCurrentAreaText()
        {
            return string.Format("Chapter {0}", (currentLevelIndex + 1));
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

        public bool TrySpendGold(int amount)
        {
            if (playerGold < amount) return false;
            playerGold -= amount;
            return true;
        }

        public void AddGold(int amount)
        {
            playerGold += amount;
        }

        public void GoToNextLevel()
        {
            StartLevel(currentLevelIndex + 1);
        }

        public void ReplayLevel()
        {
            StartLevel(currentLevelIndex);
        }

        public void StartNextWave()
        {
            waveSpawner.StartNextWave();
        }

        public void OpenFightButton()
        {
            waveData = DataManager.Instance.GetWaveData(waveSpawner.CurrentWaveIndex+2);
            if (waveData != null)
                uIGameplayCtr.ShowFightButton(waveData.SilverReward);
            else
                Debug.LogError("Null");
        }

        public void CloseFightButton()
        {
            uIGameplayCtr.HideFightButton();
        }

        private void SetDayLight(bool instant = false)
        {
            if (directionalLight == null) return;

            Vector3 dayRotation = new Vector3(60, 35, 80);

            if (instant)
            {
                directionalLight.intensity = dayLightIntensity;
                directionalLight.transform.eulerAngles = dayRotation;
            }
            else
            {
                DOTween.To(() => directionalLight.intensity,
                    x => directionalLight.intensity = x,
                    dayLightIntensity,
                    lightTransitionDuration)
                    .SetEase(Ease.InOutSine);

                directionalLight.transform.DORotate(dayRotation, lightTransitionDuration)
                    .SetEase(Ease.InOutSine);

                EnvironmentController.Instance.ChangeColorMaterialGroundToLight();
            }
        }

        private void SetNightLight(Action onComplete = null)
        {
      
            if (directionalLight == null) return;
            Vector3 nightRotation = new Vector3(60, -125, -80);

            DOTween.To(() => directionalLight.intensity,
                x => directionalLight.intensity = x,
                nightLightIntensity,
                lightTransitionDuration)
                .SetEase(Ease.InOutSine);
            directionalLight.transform.DORotate(nightRotation, lightTransitionDuration)
                .SetEase(Ease.InOutSine).OnComplete(() => {
                    onComplete?.Invoke();
                });

            EnvironmentController.Instance.ChangeColorMaterialGroundToDark();
        }

        public void UpdateProgressReward(bool hasComplete)
        {
            float progress = (waveSpawner.CurrentWaveIndex + 1) / (float)waveSpawner.waves.Count;
            Debug.LogError(progress + ": " + waveSpawner.CurrentWaveIndex + ": " + totalWaves);
            if (progress < 0.5f)
                return;

            float progressValue;

            if (progress >= 1.0f && hasComplete)
            {
                progressValue = BuildingManager.Instance.IsFullHealth() ? 1.0f : 0.5f;
            }
            else
            {
                progressValue = 0f;
            }

            float currentProgress = DataManager.Instance.GetProgressValue(currentLevelIndex);
            if (currentProgress > progressValue)
                return;
            DataManager.Instance.SaveProgressValue(currentLevelIndex, progressValue);
        }
    }
}

