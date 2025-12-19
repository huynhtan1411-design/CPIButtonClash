using CLHoma.LevelSystem;
using System.Collections.Generic;
using UISystems;
using UnityEngine;
using TemplateSystems;
#if HomaBuild
using HomaGames.HomaBelly.Internal.Analytics;
using HomaGames.HomaBelly;
using static Cinemachine.DocumentationSortingAttribute;
#endif
namespace CLHoma
{
    public class GameManager : ManualSingletonMono<GameManager>
    {
        #region Fields & Properties
        [Header("Controller")]
        [SerializeField] private UpgradesController upgradesController;
        [SerializeField] private ParticlesController particlesController;
        [SerializeField] private FloatingTextController floatingTextController;
        [SerializeField] private LevelsDatabase levelsDatabase;
        [SerializeField] private AFKCombatController afkCombatController;
        [Space]
        [Header("Game Config")]
        [SerializeField] private GameConfig gameConfig;
        private bool isGamePaused;

        public static GameConfig GameConfig => Instance.gameConfig;
        public bool IsGamePaused => isGamePaused;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeEventListeners();
            InitializeGameState();
            UIManager.instance.OnStart();
        }   

        private void OnDestroy()
        {
            UnsubscribeEvents();
            ActiveRoom.CancelSpawningEnemies();
        }
        #endregion

        #region Event Handlers
        private void InitializeEventListeners()
        {
            //UIManager.onRetryButtonPressed += RestartLevel;
            UIManager.onMenuPauseSet += InitUIPause;
            UIManager.onPauseGameSet += PauseGame;
            UIManager.onResumeGame += ResumeGame;
            UIManager.onMenuSet += OnMenuSet;
            UIManager.onLoadGame += LoadFisrtLevel;
        }

        private void UnsubscribeEvents()
        {
            //UIManager.onRetryButtonPressed -= RestartLevel;
            UIManager.onMenuPauseSet -= InitUIPause;
            UIManager.onPauseGameSet -= PauseGame;
            UIManager.onResumeGame -= ResumeGame;
            UIManager.onMenuSet -= OnMenuSet;
            UIManager.onLoadGame -= LoadFisrtLevel;
        }
        #endregion

        #region Game State Management
        private void InitialiseControllers()
        {
            upgradesController.Initialise();
            particlesController.Initialise();
            floatingTextController.Inititalise();

            LevelController.levelsDatabase = levelsDatabase;
            LevelController.Initialise();
            PlayerController.Instance.Initialise();
            //ClickerController.Instance.Initialize();
            SkillManager.Instance.Initialize();
            EnvironmentTextureController.Instance.Initialize();
        }

        private void InitializeGameState()
        {
            InitialiseControllers();
        }
        #endregion

        #region Level Management
        private void LoadFisrtLevel()
        {
            LoadLevel(0);
        }

        public void LoadLevel(int levelIndex)
        {
            LevelController.StartGameplay(levelIndex);
            PlayerController.ToggleHealthBar(true);
            afkCombatController.gameObject.SetActive(false);
#if HomaBuild
            Analytics.LevelStarted(levelIndex);
#endif
        }

        public void CompleteLevel()
        {
            UIManager.instance.SetLevelComplete();
#if HomaBuild
            Analytics.LevelCompleted();
#endif
        }

        public void FailLevel()
        {
            LevelController.OnFailLevel();
            UIManager.instance.SetGameover();
#if HomaBuild
            Analytics.LevelFailed("reason");
#endif
        }

        private void OnMenuSet()
        {
            ActiveRoom.ClearEnemies();
            PlayerController.Instance.ClearAllCharacters();
            PlayerController.ToggleHealthBar(false);
            afkCombatController.gameObject.SetActive(true);
        }
#endregion

        #region UI Management
        private void PauseGame()
        {
            isGamePaused = true;
            UIManager.instance.UIGameplayCtr.TogglePauseTimer(true);
        }

        private void ResumeGame()
        {
            isGamePaused = false;
            UIManager.instance.UIGameplayCtr.TogglePauseTimer(false);
        }

        private void InitUIPause()
        {
            var selectedHeroes = new List<string>();
            var selectedSkills = new List<string>();

            //foreach (var hero in SkillManager.Instance.GetListSkillWithType(AbilityType.Hero))
            //    selectedHeroes.Add(hero.skillData.Id);

            foreach (var skill in SkillManager.Instance.GetListSkillWithType(AbilityType.Special))
                selectedSkills.Add(skill.skillData.Id);

            UIManager.instance.UIPauseCtr.ShowUI(selectedHeroes, selectedSkills);
        }
        #endregion



    }
}