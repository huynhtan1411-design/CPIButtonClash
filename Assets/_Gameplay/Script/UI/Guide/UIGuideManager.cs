using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public enum GuideType
{
    Element,
}

public class UIGuideManager : MonoSingleton<UIGuideManager>
{
    [SerializeField] private List<UIGuide> uIGuides;
    private GuideType currentGuideType;
    private int currentGuideIndex = 0;
    private bool isGuideActive = false;

    private void Start()
    {
        // Hide all guides at start
        foreach (var guide in uIGuides)
        {
            guide.Hide();
        }
    }

    public void Show(GuideType type)
    {
        if (isGuideActive) return;

        currentGuideType = type;
        currentGuideIndex = 0;
        isGuideActive = true;
        
        var guidesOfType = uIGuides.Where(g => g.guideType == type).ToList();
        if (guidesOfType.Count > 0)
        {
            guidesOfType[0].Show();
        }
    }

    public void ShowNextGuide(GuideType type)
    {
        var guidesOfType = uIGuides.Where(g => g.guideType == type).ToList();
        if (guidesOfType.Count > 0)
        {
            guidesOfType[currentGuideIndex].Hide();
            currentGuideIndex = (currentGuideIndex + 1) % guidesOfType.Count;
            guidesOfType[currentGuideIndex].Show();
        }
    }

    public void OnGuideClosed()
    {
        isGuideActive = false;
        currentGuideIndex = 0;
    }

    public void SetGuideText(GuideType type, int index, string text)
    {
        var guidesOfType = uIGuides.Where(g => g.guideType == type).ToList();
        if (index < guidesOfType.Count)
        {
            guidesOfType[index].SetGuideText(text);
        }
    }
}