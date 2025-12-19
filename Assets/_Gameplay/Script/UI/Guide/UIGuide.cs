using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGuide : MonoBehaviour
{
    public GuideType guideType;
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI guideText;

    private void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(NextStep);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseGuide);
    }

    public void Show()
    {
        if (guidePanel != null)
            guidePanel.SetActive(true);
    }

    public void Hide()
    {
        if (guidePanel != null)
            guidePanel.SetActive(false);
    }

    public void NextStep()
    {
        UIGuideManager.Instance.ShowNextGuide(guideType);
    }

    public void CloseGuide()
    {
        Hide();
        UIGuideManager.Instance.OnGuideClosed();
    }

    public void SetGuideText(string text)
    {
        if (guideText != null)
            guideText.text = text;
    }
}