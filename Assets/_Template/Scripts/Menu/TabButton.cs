using DG.Tweening;
using UISystems;
using UnityEngine;
using UnityEngine.UI;

public class TabMenu : MonoBehaviour
{
    [System.Serializable]
    public class TabButton
    {
        public Button button;
        public GameObject selected;
        public GameObject unselected;
        public GameObject tabContent;
    }
    public RectTransform[] tabContents;
    public TabButton[] tabs;
    public RectTransform content;
    public float moveDuration = 0.6f;
    private Vector3[] tabPositions;

    private void Start()
    {
        float screenWidth = Screen.width;
        tabContents[0].anchoredPosition = new Vector2(-screenWidth * 3, 0);
        tabContents[1].anchoredPosition = new Vector2(-screenWidth * 2, 0);
        tabContents[2].anchoredPosition = new Vector2(0, 0);
        tabContents[3].anchoredPosition = new Vector2(screenWidth * 2, 0);
        tabContents[4].anchoredPosition = new Vector2(screenWidth * 3, 0);
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            tabs[i].button.onClick.AddListener(() => SelectTab(index));
        }
        tabPositions = new Vector3[]
        {
            new Vector3(Screen.width*3, 0, 0),
            new Vector3(Screen.width*2, 0, 0),
            Vector3.zero,
            new Vector3(-Screen.width * 2, 0, 0),
            new Vector3(-Screen.width * 3, 0, 0)
        };
        InitTab();
    }

    public void InitTab()
    {
        content.anchoredPosition = tabPositions[2];
        SelectTab(2);
    }

    public void SelectTab(TabButton selectedTab)
    {
        foreach (var tab in tabs)
        {
            bool isSelected = (tab == selectedTab);
            if (tab.selected != null)
                tab.selected.SetActive(isSelected);
            if (tab.unselected != null)
                tab.unselected.SetActive(!isSelected);
            if (tab.tabContent != null)
                tab.tabContent.SetActive(isSelected);
        }
    }

    public void SelectTab(int selectedIndex)
    {
        if (selectedIndex == (tabs.Length - 1))
        {
            TextNotiEffect.Instance.ShowNoti("Coming soon");
            return;
        }
        if (selectedIndex == 1)
        {
            UIManager.instance.CloseTutorial(TutorialStepID.Equipment_Menu);
            UIManager.instance.SetTutorial(TutorialStepID.Pick_Item);
        }
        if (selectedIndex == 3)
            UIManager.instance.CloseTutorial(TutorialStepID.Talent_Menu);
        Audio_Manager.instance.play("Popup");
        for (int i = 0; i < tabs.Length -1; i++)
        {
            bool isSelected = (i == selectedIndex);
            tabs[i].selected.SetActive(isSelected);
            tabs[i].unselected.SetActive(!isSelected);
            tabs[i].tabContent.SetActive(isSelected);
        }
        content.DOAnchorPos(tabPositions[selectedIndex], moveDuration).SetEase(Ease.OutQuad);
    }
}