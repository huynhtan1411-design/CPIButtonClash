using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CLHoma;
using UnityEngine.UI;
using TemplateSystems;
#if HomaBuild
using HomaGames.HomaBelly.Internal.Analytics;
using HomaGames.HomaBelly;
#endif
namespace UISystems
{
    public class UIManager : MonoBehaviour
    {
        public enum Orientation { Portrait, Landscape }
        public Orientation orientation;

        public enum GameState { MENU, MENUGAME, GAME, MENUPAUSE, LEVELCOMPLETE, GAMEOVER, SETTINGS, SHOP, NOTIBOX, TUTORIAL, LOADING, REWARDITEM }
        public static GameState gameState;

        #region Static Variables

        public static int COINS;
        public static UIManager instance;
        private static bool isInitialized = false;
        #endregion

        #region Delegates

        public delegate void SetMenuDelegate();
        public static SetMenuDelegate setMenuDelegate;

        public delegate void OnMenuSet();
        public static OnMenuSet onMenuSet;

        public delegate void OnMenuGameSet();
        public static OnMenuGameSet onMenuGameSet;

        public delegate void OnMenuPauseSet();
        public static OnMenuPauseSet onMenuPauseSet;

        public delegate void SetGameDelegate();
        public static SetGameDelegate setGameDelegate;

        public delegate void OnGameSet();
        public static OnGameSet onGameSet;

        public delegate void OnNotiBoxSet();
        public static OnNotiBoxSet onNotiBoxSet;

        public delegate void OnAddArmy(int armyCount = 1);
        public static OnAddArmy onAddArmy;

        public delegate void SetLevelCompleteDelegate(int starsCount = 3);
        public static SetLevelCompleteDelegate setLevelCompleteDelegate;

        public delegate void OnLevelCompleteSet(int starsCount = 3);
        public static OnLevelCompleteSet onLevelCompleteSet;



        public delegate void SetGameoverDelegate();
        public static SetGameoverDelegate setGameoverDelegate;

        public delegate void OnGameoverSet();
        public static OnGameoverSet onGameoverSet;


        public delegate void SetSettingsDelegate();
        public static SetSettingsDelegate setSettingsDelegate;

        public delegate void OnSettingsSet();
        public static OnSettingsSet onSettingsSet;

        public delegate void OnTutorialSet(TutorialStepID id);
        public static OnTutorialSet onTotorial;

        public delegate void OnTutorialClose(TutorialStepID id);
        public static OnTutorialClose onCloseTotorial;


        public delegate void UpdateProgressBarDelegate(float value);
        public static UpdateProgressBarDelegate updateProgressBarDelegate;



        public delegate void OnNextLevelButtonPressed();
        public static OnNextLevelButtonPressed onNextLevelButtonPressed;

        public delegate void OnRetryButtonPressed();
        public static OnRetryButtonPressed onRetryButtonPressed;

        public delegate void OnPauseGameSet();
        public static OnPauseGameSet onPauseGameSet;

        public delegate void OnResumeGame();
        public static OnResumeGame onResumeGame;

        public delegate void OnLoadGame();
        public static OnLoadGame onLoadGame;
        #endregion


        // Canvas Groups
        public CanvasGroup MENU;
        public CanvasGroup REWARDITEM;
        public CanvasGroup MENUGAME;
        public CanvasGroup MENUPAUSE;
        public CanvasGroup GAME;
        public CanvasGroup LEVELCOMPLETE;
        public CanvasGroup GAMEOVER;
        public CanvasGroup SETTINGS;
        public CanvasGroup NOTIBOX;
        public CanvasGroup TUTORIAL;
        public CanvasGroup LOADING;
        public ShopCtr shopManager;
        public CanvasGroup[] canvases;
        public List<CanvasGroup> canvasesShow = new List<CanvasGroup>();
        public List<TextMeshProUGUI> coinsTexts;
        // Menu UI
        public Text menuCoinsText;
        public bool isShowStart = true;
        public MenuGame MenuGameCtr = null;
        public UIGameCtr UIGameplayCtr = null;
        public UIMenuPause UIPauseCtr = null;
        public MenuLevel MenuLevelCtr = null;
        // Game UI
        public Slider progressBar;
        public ProgressUIManager progressNot;
        public Text gameCoinsText;
        public TextMeshProUGUI levelText;
        public float timerLoading = 0;

        // Shop UI
        public Text shopCoinsText;

        // Level Complete UI
        public Text levelCompleteCoinsText;
        public List<ParticleSystem> effectCompletes = null;
        public UILevelComplete UILevelWin;
        public UILevelComplete UILevelLose;
        public UIRewardHero uIRewardHero;
        public UILoading UILoading;
        private bool isLoading = false;
        public static object Instance { get; internal set; }

        private void Awake()
        {
            if (instance == null)
                instance = this;
            if(MENUGAME != null)
                MenuGameCtr = MENUGAME.GetComponent<MenuGame>();
            if (GAME != null)
                UIGameplayCtr = GAME.GetComponent<UIGameCtr>();
            if (MENUPAUSE != null)
                UIPauseCtr = MENUPAUSE.GetComponent<UIMenuPause>();
            if (LEVELCOMPLETE)
                UILevelWin = LEVELCOMPLETE.GetComponent<UILevelComplete>();
            if (GAMEOVER)
                UILevelLose = GAMEOVER.GetComponent<UILevelComplete>();
            if (REWARDITEM)
                uIRewardHero = REWARDITEM.GetComponent<UIRewardHero>();
            if (LOADING)
                UILoading = LOADING.GetComponent<UILoading>();
            if (MENU)
                MenuLevelCtr = MENU.GetComponent<MenuLevel>();
            
            // Get the coins amount
            //COINS = PlayerPrefsManager.GetCoins();
            //if (COINS < 0)
            //    PlayerPrefsManager.SaveCoins(10);
            //COINS = PlayerPrefsManager.GetCoins();
            UpdateCoins();
        }

        // Start is called before the first frame update
        public void OnStart()
        {
            // Store the canvases
            canvases = new CanvasGroup[] { MENU, MENUGAME, MENUPAUSE, GAME, LEVELCOMPLETE, GAMEOVER, SETTINGS , NOTIBOX, LOADING , REWARDITEM };

            // Configure the delegates
            ConfigureDelegates();
            // Set the game at start
            if (!isInitialized)
            {
                isInitialized = true;
                int isFirst = PlayerPrefs.GetInt(TYPELEVEL.First.ToString(), 0);
                if (isFirst != 0 && isShowStart)
                    SetMenu();
                else
                {
                    Debug.LogError("First");
                    PlayerPrefs.SetInt(TYPELEVEL.First.ToString(), 1);
                    SetGame();
                    onLoadGame?.Invoke();
                }
            }
        }

        private void ConfigureDelegates()
        {
            // Basic events
            setMenuDelegate += SetMenu;
            setGameDelegate += SetGame;
            setLevelCompleteDelegate += SetLevelComplete;
            setGameoverDelegate += SetGameover;
            setSettingsDelegate += SetSettings;

            // Progress bar events
            updateProgressBarDelegate += UpdateProgressBar;
        }

        private void OnDestroy()
        {

            // Basic events
            setMenuDelegate -= SetMenu;
            setGameDelegate -= SetGame;
            setLevelCompleteDelegate -= SetLevelComplete;
            setGameoverDelegate -= SetGameover;
            setSettingsDelegate -= SetSettings;

            // Progress bar events
            updateProgressBarDelegate -= UpdateProgressBar;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                UIManager.instance.SetGameover();
            }
        }

        public void SetMenu()
        {
            onPauseGameSet?.Invoke();
            if (timerLoading > 0)
            {
                SetLoading(timerLoading, () =>
                {
                    gameState = GameState.MENU;
                    Utils.HideAllCGs(canvases, MENU);
                });
            }
            else
            {
                gameState = GameState.MENU;
                Utils.HideAllCGs(canvases, MENU);
            }
            canvasesShow.Clear();
            Utils.DisableCG(NOTIBOX);
            onMenuSet?.Invoke();

            Audio_Manager.instance.play("bgm_menu_A");
#if HomaBuild
            Analytics.MainMenuLoaded();
#endif
        }

        public void SetMenuGame()
        {
            onPauseGameSet?.Invoke();
            gameState = GameState.MENUGAME;
            Utils.HideAllCGs(canvases, MENUGAME);

            // Invoke the delegate
            onMenuGameSet?.Invoke();
            //Sound
        }

        public void SetMenuPause()
        {
            onPauseGameSet?.Invoke();
            gameState = GameState.MENUPAUSE;
            Utils.HideAllCGs(canvases, MENUPAUSE);

            // Invoke the delegate
            onMenuPauseSet?.Invoke();
            //Sound
            Audio_Manager.instance.play("Click");

        }
        public void SetResume()
        {
            onResumeGame?.Invoke();
            gameState = GameState.GAME;
            Utils.HideAllCGs(canvases, GAME);
            Audio_Manager.instance.play("Click");
        }
        public void SetNotiBox()
        {
            gameState = GameState.NOTIBOX;
            Utils.EnableCG(NOTIBOX);
            canvasesShow.Add(NOTIBOX);
            //Utils.HideAllCGs(canvases, NOTIBOX);

            // Invoke the delegate
            onNotiBoxSet?.Invoke();
            //Sound
        }

        public void SetGame()
        {
            UpdateCoins();
            onResumeGame?.Invoke();
            if (timerLoading > 0)
            {
                SetLoading(timerLoading, () =>
                {
                    gameState = GameState.GAME;
                    Utils.HideAllCGs(canvases, GAME);
                });
            }
            else
            {
                gameState = GameState.GAME;
                Utils.HideAllCGs(canvases, GAME);
            }

            // Invoke the delegate
            onGameSet?.Invoke();
            // Reset the progress bar
            progressBar.value = 0;

            // Update the level text
            levelText.text = "Level " + (PlayerPrefsManager.GetLevel() + 1);
#if HomaBuild
            Analytics.GameplayStarted();
#endif
        }

        public void AddArmy(int count)
        {
            onAddArmy?.Invoke();
        }

        public void SetLevelComplete(int starsCount = 3)
        {
            onPauseGameSet?.Invoke();
            gameState = GameState.LEVELCOMPLETE;
            foreach (var effect in effectCompletes)
                effect.Play();
            Utils.HideAllCGs(canvases, LEVELCOMPLETE);

            // Invoke the delegate
            onLevelCompleteSet?.Invoke(starsCount);
            Audio_Manager.instance.play("Level_Complete");
            //Invoke("AdsControl", 2f);
        }
        public void SetRewardHero(string id)
        {
            gameState = GameState.REWARDITEM;
            Utils.HideAllCGs(canvases, REWARDITEM);
            uIRewardHero.ShowRewardHeroById(id);
            //Audio_Manager.instance.play("Level_Complete");
        }
        private void AdsControl()
        {
            //AdManager.instance.ShowInterstitialAd();
        }

        public void SetGameover()
        {
            onPauseGameSet?.Invoke();
            gameState = GameState.GAMEOVER;
            Utils.HideAllCGs(canvases, GAMEOVER);

            // Invoke the delegate
            onGameoverSet?.Invoke();
            Audio_Manager.instance.play("Game_Over");
            //Invoke("AdsControl", 2f);
        }


        public void SetSettings()
        {
            gameState = GameState.SETTINGS;
            Utils.EnableCG(MENU);
            Utils.HideAllCGs(canvases, SETTINGS);
            Audio_Manager.instance.play("Click");
            // Invoke the delegate
            onSettingsSet?.Invoke();
        }

        public void SetSettingsOnMenu()
        {
            gameState = GameState.SETTINGS;
            Utils.EnableCG(MENU);
            Utils.EnableCG(SETTINGS);
            canvasesShow.Add(MENU);
            canvasesShow.Add(SETTINGS);
            //Utils.HideAllCGs(canvases, canvasesShow.ToArray());
            Audio_Manager.instance.play("Click");
            // Invoke the delegate
            onSettingsSet?.Invoke();
        }

        public void SetTutorial(TutorialStepID id)
        {
            Utils.EnableCG(TUTORIAL, false);
            onTotorial?.Invoke(id);
        }
        public void CloseTutorial(TutorialStepID id)
        {
            Utils.DisableCG(TUTORIAL);
            onCloseTotorial?.Invoke(id);
        }

        public void SetLoading(float timer, Action action = null)
        {
            Debug.LogError("SetLoading");
            gameState = GameState.LOADING;
            Utils.HideAllCGs(canvases, LOADING);
            UILoading.ShowLoading();
            CallWaitAction(timer, () =>
            {
                action?.Invoke();
            });
        }

        public void SetShop()
        {
            gameState = GameState.SHOP;

            // Enable the shop gameobject
            shopManager.gameObject.SetActive(true);

            // Hide all the other canvases
            Utils.HideAllCGs(canvases);
        }


        public void CloseShop()
        {
            // Disable the shop object
            shopManager.gameObject.SetActive(false);

            // Get back to the menu
            SetMenu();
        }

        public void NextLevelButtonCallback()
        {
            Audio_Manager.instance.play("Click");
            SetGame();

            // Invoke the next button delegate
            onNextLevelButtonPressed?.Invoke();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void RetryButtonCallback()
        {
            Audio_Manager.instance.play("Click");
            SetGame();
            gameState = UIManager.GameState.GAME;
            // Invoke the retry button delegate
            onRetryButtonPressed?.Invoke();
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void CloseSettings()
        {
            if (canvasesShow.Count > 0)
            {
                if (canvasesShow.Contains(SETTINGS))
                    canvasesShow.Remove(SETTINGS);
                if (canvasesShow.Contains(MENU))
                {
                    canvasesShow.Remove(MENU);
                    SetMenu();
                }
                else
                    SetGame();
            }
            else
            {
                SetGame();
            }

        }
        public void CloseMenu(bool isLd = false)
        {
            isLoading = isLd;
            SetGame();
            onRetryButtonPressed?.Invoke();
            //onResumeGame?.Invoke();
        }
        public void UpdateProgressBar(float value)
        {
            progressBar.value = value;
        }

        public void UpdateCoins()
        {
            //menuCoinsText.text = Utils.FormatAmountString(COINS);
            //gameCoinsText.text = menuCoinsText.text;
            //if (shopCoinsText != null)
            //    shopCoinsText.text = menuCoinsText.text;
            //levelCompleteCoinsText.text = menuCoinsText.text;
            foreach (var coninText in coinsTexts)
                if(coninText != null)
                coninText.text = TemplateSystems.DataManager.COINS.ToString();

            //UpdateNotice();
        }
        public void UpdateNotice()
        {
            try
            {
                bool isNoticeTalent = DataManager.Instance.CanUpgradeTalent();
                if (isNoticeTalent)
                    UINoticeManager<string>.Instance.UpdateInfo(NoticeKey.Talent.ToString(), NoticeStatus.Red);
                else
                    UINoticeManager<string>.Instance.UpdateInfo(NoticeKey.Talent.ToString(), NoticeStatus.None);
            }
            catch (Exception e)
            {
                Debug.LogError("UpdateNotice: " + e);
            }
        }

        #region Static Methods

        public static void AddCoins(int amount)
        {
            // Increase the amount of coins
            COINS += amount;

            // Update the coins
            instance.UpdateCoins();

            // Save the amount of coins
            PlayerPrefsManager.SaveCoins(COINS);
        }

        public static bool IsGame()
        {
            return gameState == GameState.GAME;
        }

        public static bool IsLevelComplete()
        {
            return gameState == GameState.LEVELCOMPLETE;
        }

        public static bool IsGameover()
        {
            return gameState == GameState.GAMEOVER;
        }

        public static void CallWaitAction(float timer, Action action)
        {
            instance.StartCoroutine(instance.WaitAction(timer, action));
        }
        IEnumerator WaitAction(float timer, Action action)
        {
            yield return new WaitForSeconds(timer);
            action?.Invoke();
        }

        #endregion
        public async void LoadIcon(string id, Image image)
        {
            string iconAddress = "icons/" + id + ".png";
            var handle = Addressables.LoadAssetAsync<Sprite>(iconAddress);
            await handle.Task;

            if (handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded)
            {
                image.sprite = handle.Result;
                image.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Failed to load icon at " + iconAddress);
                image.gameObject.SetActive(false);
            }
        }
        public async void LoadIconEquipments(string id, Image image)
        {
            if (image == null)
                return;
            string idImage = TemplateSystems.DataManager.Instance.GetImageID(id);
            string iconAddress = "equipments/" + idImage + ".png";
            var handle = Addressables.LoadAssetAsync<Sprite>(iconAddress);
            await handle.Task;

            if (handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (image == null)
                    return;
                image.sprite = handle.Result;
                image.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Failed to load icon at " + iconAddress);
                image.gameObject.SetActive(false);
            }
        }


        public void SetTutorialElement() {
            SetTutorial(TutorialStepID.ELEMENTAL);
        }

        public void CloseTutorialElement()
        {
            CloseTutorial(TutorialStepID.ELEMENTAL);
        }
    }
}