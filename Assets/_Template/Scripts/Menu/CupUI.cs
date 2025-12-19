using TMPro;
using UnityEngine;

public class CupUI : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public GameObject completedSkin;
    public GameObject currentOutline;
    public GameObject lockedSkin;

    public void SetLevel(int levelNumber)
    {
        levelText.text = "" + levelNumber;
    }

    public void SetCompletedSkin()
    {
        completedSkin.SetActive(true);
        currentOutline.SetActive(false);
        lockedSkin.SetActive(false);
        levelText.gameObject.SetActive(false);
    }

    public void SetCurrentSkin()
    {
        completedSkin.SetActive(false);
        currentOutline.SetActive(true);
        lockedSkin.SetActive(false);
    }

    public void SetLockedSkin()
    {
        completedSkin.SetActive(false);
        currentOutline.SetActive(false);
        lockedSkin.SetActive(true);
    }
}
