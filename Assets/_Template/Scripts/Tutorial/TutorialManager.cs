using System.Collections.Generic;
using UISystems;
using UnityEngine;
using UnityEngine.UI;
#if HomaBuild
using HomaGames.HomaBelly.Internal.Analytics;
using HomaGames.HomaBelly;
#endif

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<TutorialStep> _listTutorial = null;
    [SerializeField] private Image _lockObj = null;
    public static TutorialManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            UIManager.onTotorial = SetTutorial;
            UIManager.onCloseTotorial = HideAllTutorials;
        }
    }

    public void SetTutorial(TutorialStepID stepID)
    {
#if TUTORIAL
        int id = stepID.GetHashCode();
        if (GetTutorial(id) == 1)
        {
            return;
        }
        _lockObj.enabled = true;
        foreach (var tutorial in _listTutorial)
        {
            if (tutorial != null)
                tutorial.gameObject.SetActive(false);
        }

        if (_listTutorial[id] != null)
        {
            _listTutorial[id].gameObject.SetActive(true);
            TutorialStep step = _listTutorial[id].gameObject.GetComponent<TutorialStep>();
            step.onUnmaskSequenceComplete.AddListener(() => {
                _lockObj.enabled = false;
            });
        }
        UIManager.onPauseGameSet?.Invoke();
#if HomaBuild
        Analytics.TutorialStepStarted(id);
#endif
        Debug.Log("Tutorial Step Started: " + stepID);
#endif
    }

    public void HideAllTutorials(TutorialStepID stepID)
    {
#if TUTORIAL
        int id = stepID.GetHashCode();
        if (GetTutorial(id) == 1)
        {
            return;
        }
        SaveTutorial(id);
        foreach (var tutorial in _listTutorial)
        {
            if (tutorial != null)
                tutorial.gameObject.SetActive(false);
        }
        UIManager.onResumeGame?.Invoke();
#if HomaBuild
        Analytics.TutorialStepCompleted();
#endif
        Debug.Log("Tutorial Step Completed: " + stepID);
#endif
    }

    public static int GetTutorial(int id)
    {
        return PlayerPrefsManager.GetTutorial(id);
    }

    public static void SaveTutorial(int id)
    {
        PlayerPrefsManager.SaveTutorial(id);
    }
}
