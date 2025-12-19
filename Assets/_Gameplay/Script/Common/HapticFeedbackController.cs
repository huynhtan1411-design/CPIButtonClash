using MoreMountains.NiceVibrations;
using System;
using UnityEngine;

public class HapticFeedbackController : MonoBehaviour
{
    public static event Action<bool> OnHapticsEnabledChanged = delegate { };

    private static HapticFeedbackController _instance;

    private float _hapticTimer = 0f;
    private bool _currentlyActive = false;
    private bool _hapticsPaused = false;

    private const float _hapticMinimumDelay = 0.3f;

    private void Awake()
    {
        _instance = this;
        _currentlyActive = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;
    }

    private void Update()
    {
        _hapticTimer -= Time.deltaTime;
    }

    private void Start()
    {
#if UNITY_IOS
            MMVibrationManager.iOSInitializeHaptics( );
#endif
    }
    public static void TriggerHaptics(HapticTypes type, bool force = false)
    {
        if (_instance._hapticsPaused)
            return;

        if (!_instance._currentlyActive)
            return;

        if (_instance._hapticTimer > 0 && !force)
            return;

        MMVibrationManager.Haptic(type);
        _instance._hapticTimer = _hapticMinimumDelay;
    }
}
