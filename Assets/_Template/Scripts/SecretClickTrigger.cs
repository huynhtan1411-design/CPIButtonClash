using UnityEngine;
using UnityEngine.UI;
public class SecretClickTrigger : MonoBehaviour
{
    public GameObject testObject;
    public Button btnActive;
    private int clickCount = 0;
    private float lastClickTime = 0f;
    private float resetTime = 1.5f;
    private int requiredClicks = 10;

    void Start()
    {
        if (btnActive != null)
        {
            btnActive.onClick.AddListener(OnSettingClicked);
        }

        testObject.SetActive(false);
    }

    private void OnSettingClicked()
    {
        if (Time.time - lastClickTime > resetTime)
        {
            clickCount = 0;
        }

        lastClickTime = Time.time;
        clickCount++;

        if (clickCount >= requiredClicks)
        {
            testObject.SetActive(true);
            Debug.Log("Secret Unlocked! Showing Test Object.");
            clickCount = 0;
        }
    }
}