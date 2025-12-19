using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuPause : MonoBehaviour
{
    [SerializeField] private ListViewManager listViewHeros;
    [SerializeField] private ListViewManager listViewSkills;

    [Header("Guide")]
    [SerializeField] private GameObject btnGuide;
    [SerializeField] private GameObject elementGuide;
    public void ShowUI(List<string> idHeros, List<string> idSkills)
    {
        //listViewHeros.UpdateList(idHeros);
        listViewSkills.UpdateList(idSkills);
        CheckShowButtonGuide();
    }

    private void CheckShowButtonGuide()
    {
        if (TutorialManager.GetTutorial(TutorialStepID.ELEMENTAL.GetHashCode()) == 1)
        {
            btnGuide.gameObject.SetActive(true);
        }
        else
        {
            btnGuide.gameObject.SetActive(false);
        }
    }

    public void ShowGuideElemental()
    {
        elementGuide.SetActive(true);
    }
}
