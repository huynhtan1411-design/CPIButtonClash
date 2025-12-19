using System.Collections.Generic;
using UISystems;
using UnityEngine;
using TemplateSystems;
using TMPro;
using UnityEngine.UI;
using CLHoma;
using ButtonClash.UI;
using System;
using System.Collections;
using DG.Tweening;

public class MenuLevel : MonoBehaviour
{
    public Transform cupContainer;
    public GameObject cupPrefab;
    public int totalLevels = 100;
    public int levelsToShow = 5;
    [SerializeField] private TextMeshProUGUI textMeshProUGUICoin = null;
    [SerializeField] TabMenu tabMenu = null;
    [SerializeField] GameObject panelLockMenu = null;
    [SerializeField] TextMeshProUGUI textLevel = null;
    [SerializeField] private List<GameObject> backgrounds = new List<GameObject>();
    [SerializeField] private List<GameObject> cupObjects = new List<GameObject>();

    [Header("Button")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button previousLevelButton;

    [Header("ProgressUI")]
    [SerializeField] private ProgressUIManager progressUIManager;

    [Header("Reward Popup")]
    [SerializeField] private GameObject rewardPopup;
    [SerializeField] private UIItemList itemRewards;
    [SerializeField] private Button closeRewardButton;
    [SerializeField] private CanvasGroup canvasGroup;


    private int lastCompletedLevel;
    private int levelIndex;

    public Action OnRewardClosed { get; set; }

    private void Awake()
    {
        UIManager.onMenuSet = SetMenu;

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevel);

        if (previousLevelButton != null)
            previousLevelButton.onClick.AddListener(OnPreviousLevel);

        if (closeRewardButton != null)
            closeRewardButton.onClick.AddListener(CloseRewardPopup);

        rewardPopup.SetActive(false);

        if (rewardPopup != null && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    void SetMenu()
    {
        textMeshProUGUICoin.text = DataManager.GetCoins().ToString();
        levelIndex = DataManager.Instance.GetLevel();
        lastCompletedLevel = DataManager.Instance.GetLevel();
        textLevel.text = "Chapter " + (levelIndex + 1);
        SetBackground(levelIndex);
        LoadProgressReward(levelIndex);
        UIManager.instance.SetTutorial(TutorialStepID.Equipment_Menu);
    }

    public void SetBackground(int level)
    {
        if (backgrounds == null || backgrounds.Count == 0)
        {
            return;
        }

        int bgIndex = (level) % backgrounds.Count;

        foreach (var bg in backgrounds)
        {
            if (bg != null)
                bg.SetActive(false);
        }

        if (backgrounds[bgIndex] != null)
        {
            backgrounds[bgIndex].SetActive(true);
        }
        //EnvironmentTextureController.Instance.ChangeTextureByChapter(levelIndex);
    }

    private void LoadProgressReward(int level)
    {
        if (progressUIManager != null)
        {
            progressUIManager.ViewProgressRewardForLevel(level);
        }
    }

    private void OnNextLevel()
    {
        if (levelIndex < lastCompletedLevel)
        {
            levelIndex++;
            textLevel.text = "Chapter " + (levelIndex + 1);
            SetBackground(levelIndex);
            LoadProgressReward(levelIndex);
        }
    }

    private void OnPreviousLevel()
    {
        if (levelIndex > 0)
        {
            levelIndex--;
            textLevel.text = "Chapter " + (levelIndex + 1);
            SetBackground(levelIndex);
            LoadProgressReward(levelIndex);
        }
    }
    public void OnStartGame()
    {
        //GameManager.Instance.LoadLevel(levelIndex);
        UIManager.onLoadGame();
        UIManager.instance.SetGame();
    }


    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.G))
        {
            int randomValue = UnityEngine.Random.Range(0, 3);
            switch (randomValue)
            {
                case 0:
                    progressUIManager.SetPartialClear(levelIndex);
                    break;
                case 1:
                    progressUIManager.SetFullClear(levelIndex);
                    break;
                case 2:
                    progressUIManager.SetPerfectClear(levelIndex);
                    break;
                default:
                    break;
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ShowReward(null);
        }
#endif
    }

    public void ShowReward(List<ItemInfo> itemInfos)
    {
        if (rewardPopup != null)
        {
            rewardPopup.SetActive(true);
            canvasGroup.alpha = 0f;

            rewardPopup.transform.localScale = Vector3.one * 0.1f;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1f, 0.4f).SetEase(Ease.OutQuad));
            sequence.Join(rewardPopup.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        }
        itemRewards.Setup(itemInfos, false);
    }

    public void CloseRewardPopup()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(0f, 0.4f).SetEase(Ease.InQuad));
        sequence.Join(rewardPopup.transform.DOScale(Vector3.one * 0.6f, 0.5f).SetEase(Ease.InBack));

        sequence.OnComplete(() =>
        {
            if (rewardPopup != null)
                rewardPopup.SetActive(false);

            if (OnRewardClosed != null)
            {
                OnRewardClosed.Invoke();
                OnRewardClosed = null;
            }
        });
    }

    public void LockMenuUI()
    {
        panelLockMenu.SetActive(true);
    }

    public void UnLockMenuUI()
    {
        panelLockMenu.SetActive(false);
    }
}