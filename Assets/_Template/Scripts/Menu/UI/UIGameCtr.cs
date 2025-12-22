using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UISystems;
using DG.Tweening;
using UnityEngine.UI;
using CLHoma;
using System.Collections;
using TemplateSystems;
using WD;
public class UIGameCtr : MonoBehaviour
{
    [SerializeField] private ListViewManager listViewSkills;
    [SerializeField] private TextMeshProUGUI _textTimer;
    [SerializeField] private TextMeshProUGUI _textGoldReward;
    [SerializeField] private TextMeshProUGUI _textEnemyKill;
    [SerializeField] private TextMeshProUGUI _textResourcePerSecond;
    [SerializeField] private TextMeshProUGUI _textWave;
    [SerializeField] private TextMeshProUGUI _textPlayerGold;
    [SerializeField] private Transform UIWarningBoss;
    [SerializeField] private MergeCardHeros mergeCardHerosCtr;
    [SerializeField] private TextMeshProUGUI _textProgress;
    [SerializeField] private Slider _experienceSlider;
    [SerializeField] private Button _fightButton;
    [SerializeField] private TextMeshProUGUI _fightTextButton;
    [SerializeField] private SpawnDirectionUI spawnDirectionUI;
    [SerializeField] private FloatingJoystick floatingJoystick;

    [Header("Crafting")]
    [SerializeField] private RectTransform buttonCrafting;
    [SerializeField] private RectTransform icon_Upgrade;
    [SerializeField] private UICraftBuildingPanel craftBuildingPanel;

    [SerializeField] private CanvasGroup _warnignHealth;
    private PlayerStatsManager playerStats;
    private float _remainingTime = 0f;
    private float _initialTime = 0f;
    private bool _isPause = false;
    private bool _isTimerRunning = false;
    private Transform _currentCraftingTarget;
    private bool _isTrackingCraftingButton;
    private Coroutine _craftingTrackingCoroutine;
    private Tweener _healthWarningTweener;

    public ListViewManager ListViewSkills => listViewSkills;

    public Button FightButton { get => _fightButton; }
    public SpawnDirectionUI SpawnDirectionUI => spawnDirectionUI;
    public FloatingJoystick FloatingJoystick => floatingJoystick;

    public TextMeshProUGUI TextWave { get => _textWave; set => _textWave = value; }

    public event Action OnTimerEnd;

    #region Unity Lifecycle

    private void Start()
    {
        UIManager.onGameSet += InitUIGame;
        UIManager.onMenuSet += Reset;
        UIManager.onGameoverSet += Reset;
        UIManager.onLevelCompleteSet += ResetComplete;
        StartCoroutine(DelayedInit());

        _fightButton.onClick.AddListener(delegate { ToggleButtonCrafting(false); });
    }

    public void Init()
    {
        playerStats = PlayerController.StatsManager;
        if (WD.GameManager.Instance != null)
        {
            UpdatePlayerGoldUI(WD.GameManager.Instance.PlayerGold);
        }
    }

    private IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(0.2f);
        Init();
        UpdateExperienceSlider();
    }


    private void Update()
    {
        if (!_isTimerRunning || _isPause) return;

        _remainingTime -= Time.deltaTime;
        UpdateTimerDisplay();

        if (_remainingTime <= 0)
        {
            StopTimer();
            OnTimerComplete();
        }
    }

    private void OnEnable()
    {
        Debug.Log("[UIGameCtr] OnEnable subscribe events");
        BuildingManager.OnResourceUpdated += UpdateResourdUI;
        if (WD.GameManager.Instance != null)
        {
            WD.GameManager.Instance.OnPlayerGoldChanged += UpdatePlayerGoldUI;
            UpdatePlayerGoldUI(WD.GameManager.Instance.PlayerGold);
        }
    }

    private void OnDisable()
    {
        Debug.Log("[UIGameCtr] OnDisable unsubscribe events");
        BuildingManager.OnResourceUpdated -= UpdateResourdUI;
        if (WD.GameManager.Instance != null)
            WD.GameManager.Instance.OnPlayerGoldChanged -= UpdatePlayerGoldUI;
    }

    private void OnDestroy()
    {
        UIManager.onGameSet -= InitUIGame;
        UIManager.onMenuSet -= Reset;
        UIManager.onGameoverSet -= Reset;
        UIManager.onLevelCompleteSet -= ResetComplete;
    }

    #endregion

    #region Timer Methods

    public void SetupTimer(float seconds)
    {
        _initialTime = Mathf.Max(0, seconds);
        _remainingTime = _initialTime;
        _isTimerRunning = true;
        _isPause = false;
        UpdateTimerDisplay();
        UpdateExperienceSlider();
    }

    public void StopTimer()
    {
        _isTimerRunning = false;
        _remainingTime = 0;
        UpdateTimerDisplay();
    }

    public void TogglePauseTimer(bool pause)
    {
        _isPause = pause;
    }

    public float GetRemainingTime()
    {
        return _remainingTime;
    }

    public float GetTimerProgress()
    {
        return _initialTime > 0 ? _remainingTime / _initialTime : 0;
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(_remainingTime / 60);
        int seconds = Mathf.FloorToInt(_remainingTime % 60);
        _textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnTimerComplete()
    {
        OnTimerEnd?.Invoke();
    }

    public string GetSurvivalTime()
    {
        float surTime = _initialTime - _remainingTime;
        int minutes = Mathf.FloorToInt(surTime / 60);
        int seconds = Mathf.FloorToInt(surTime % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public int GetSurvivalTimeInt()
    {
        float surTime = _initialTime - _remainingTime;
        return Mathf.FloorToInt(surTime);
    }
    #endregion

    #region UI Methods

    public void Reset()
    {
        UIManager.instance.MenuGameCtr.SelectedSkills.Clear();
        StopTimer();
    }

    public void ResetComplete(int count)
    {
        Reset();
    }

    void InitUIGame()
    {     
        List<string> idItems = UIManager.instance.MenuGameCtr.SelectedSkills;
        ListViewSkills.UpdateList(idItems);
    }

    public void UpdateResourdUI(int gold, int enemyKill)
    {
        _textGoldReward.text = gold.ToString();
        _textEnemyKill.text = enemyKill.ToString();
        if (_textPlayerGold != null)
        {
            _textPlayerGold.text = gold.ToString();
            Debug.Log($"[UIGameCtr] UpdateResourdUI -> sync player gold text to {gold}");
        }
    }

    public void UpdateResourcePerSecUI(float resPerSecond)
    {
        bool shouldShow = resPerSecond > 0;
        _textResourcePerSecond.gameObject.SetActive(shouldShow);

        if (shouldShow)
        {
            _textResourcePerSecond.text = $"{resPerSecond} / sec";
        }
    }

    public void UpdatePlayerGoldUI(int gold)
    {
        if (_textPlayerGold != null)
        {
            _textPlayerGold.text = gold.ToString();
            Debug.Log($"[UIGameCtr] UpdatePlayerGoldUI -> {gold}");
        }
        else
        {
            Debug.LogWarning($"[UIGameCtr] _textPlayerGold not assigned. Gold value={gold}");
        }
    }

    public void ToggleUIBossWarning(bool show)
    {
        if (UIWarningBoss == null) return;
        
        UIWarningBoss.gameObject.SetActive(show);

        if (show)
        {
            Audio_Manager.instance.play("sfx_ui_alert");
            DOVirtual.DelayedCall(4f, () => ToggleUIBossWarning(false));
        }
    }
    private void UpdateExperienceSlider()
    {
        if (_experienceSlider == null) return;

        //double requiredExperience = PlayerController.StatsManager.GetExperienceRequiredForNextLevel();
        //int Progress = (int)((PlayerController.StatsManager.CurrentExperience/requiredExperience)*100);
        //_textProgress.text = Progress + "%";
        //Debug.LogError(PlayerController.StatsManager.CurrentExperience + "/" + requiredExperience);
        //_experienceSlider.value = (float)(PlayerController.StatsManager.CurrentExperience / requiredExperience);
    }

    public void AddExperience(TypeRarityHero typeRarity)
    {
        double ex = 0;

        switch (typeRarity)
        {
            case TypeRarityHero.Common:
                ex = 1;
                break;
            case TypeRarityHero.Rare: 
                ex = 2;
                break;
            case TypeRarityHero.Epic:
                ex = 5;
                break;
            case TypeRarityHero.Legendary:
                ex = 10;
                break;
            case TypeRarityHero.Godlike: 
                ex = 20;
                break;
            default:
                Debug.LogWarning($"Unhandled TypeRarityHero: {typeRarity}");
                break;
        }

        if (PlayerController.StatsManager != null)
        {
            PlayerController.StatsManager.AddExperience(ex);
            UpdateExperienceSlider();
        }
        else
        {
            Debug.LogError("PlayerController.StatsManager is null");
        }
    }

    public void ToggleButtonCrafting(bool active, Transform target = null)
    {
        if (buttonCrafting == null) return;

        _currentCraftingTarget = active ? target : null;
        buttonCrafting.gameObject.SetActive(active);

        if (active)
        {
            if (_craftingTrackingCoroutine != null)
                StopCoroutine(_craftingTrackingCoroutine);
            _craftingTrackingCoroutine = StartCoroutine(TrackCraftingTarget());


            if (BuildingManager.Instance.CurrentActiceSlotInteractive.IsEmpty())
            {
                icon_Upgrade.gameObject.SetActive(false);
            }
            else
                icon_Upgrade.gameObject.SetActive(true);
        }
        else
        {
            if (_craftingTrackingCoroutine != null)
            {
                StopCoroutine(_craftingTrackingCoroutine);
                _craftingTrackingCoroutine = null;
            }
        }
    }

    private IEnumerator TrackCraftingTarget()
    {
        while (_currentCraftingTarget != null && buttonCrafting.gameObject.activeSelf)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(_currentCraftingTarget.position);
            
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                buttonCrafting.parent as RectTransform,
                screenPos,
                null,
                out Vector2 localPoint))
            {
                buttonCrafting.localPosition = localPoint;
            }

            yield return null;
        }
    }

    public void OnButtonCraftingClick()
    {
        var slot = BuildingManager.Instance.CurrentActiceSlotInteractive;
        if(slot.IsEmpty())
            craftBuildingPanel.Show(slot.SlotType);
        else
        {
            craftBuildingPanel.Show(slot.BuildingBehaviour.BuildingConfig, slot.BuildingBehaviour.CurrentLevel);
        }
        ToggleButtonCrafting(false);
        Audio_Manager.instance.play("Click");
    }

    public void ShowHealthWarning(float duration = 0.4f)
    {
        if (_warnignHealth == null) return;

        if (_healthWarningTweener != null)
        {
            _healthWarningTweener.Kill();
        }

        _warnignHealth.gameObject.SetActive(true);
        _healthWarningTweener = _warnignHealth.DOFade(1f, duration)
            .OnComplete(() => 
            {
                _warnignHealth.DOFade(0.25f, 0.25f)
                    .OnComplete(() =>
                    {
                        _healthWarningTweener = null;
                        _warnignHealth.gameObject.SetActive(false);
                    });
            });
    }

    public void ShowFightButton(int silver)
    {
        _fightTextButton.text = "+ " + silver;
        // _fightButton.gameObject.SetActive(true);
    }
    public void HideFightButton()
    {
        _fightButton.gameObject.SetActive(false);
    }
    #endregion
}
