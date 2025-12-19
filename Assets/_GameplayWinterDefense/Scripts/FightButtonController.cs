using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FightButtonController : MonoBehaviour
{
    [Tooltip("Reference to the WaveSpawner to start enemy waves")]
    public WaveSpawner waveSpawner;

    private Button fightButton;

    private void Awake()
    {
        // Get the Button component attached to this GameObject
        fightButton = GetComponent<Button>();

        if (waveSpawner == null)
        {
            Debug.LogError("WaveSpawner is not assigned on FightButtonController!", this);
            fightButton.interactable = false;
            return;
        }

        // Register click event
        fightButton.onClick.AddListener(OnFightButtonClicked);
    }

    private void OnDestroy()
    {
        // Unregister click event to prevent memory leaks
        fightButton.onClick.RemoveListener(OnFightButtonClicked);
    }

    /// <summary>
    /// Called when the Fight button is clicked
    /// </summary>
    private void OnFightButtonClicked()
    {
        // Disable the button to prevent multiple clicks
        fightButton.interactable = false;

        // Trigger the next wave in WaveSpawner
        waveSpawner.StartNextWave();
    }

    /// <summary>
    /// Enables the Fight button (e.g., after build phase ends)
    /// </summary>
    public void EnableFightButton()
    {
        fightButton.interactable = true;
    }
}