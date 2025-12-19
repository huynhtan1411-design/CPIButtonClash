using MoreMountains.NiceVibrations;
using UISystems;
using UnityEngine;

public class Vibration_Manager : MonoBehaviour
{
    public bool IsVibration = true;
    public static Vibration_Manager instance;
    void Awake()
    {
        if (instance == null)
            instance = this;
        // DontDestroyOnLoad(this);

    }

    private void Start()
    {

    }

    public void GetData()
    {
        IsVibration = PlayerPrefsManager.GetVibrationState() == 1 ? true : false;
    }

    public void SetVibration(bool value)
    {
        IsVibration = value;
        PlayerPrefsManager.SetVibrationState(IsVibration ? 1 : 0);
        if (IsVibration)
            HapticFeedbackController.TriggerHaptics(HapticTypes.Selection);
    }
    public void Vibrate(HapticTypes haptic)
    {
        if (!IsVibration) return;
#if (UNITY_ANDROID || UNITY_IOS)
        HapticFeedbackController.TriggerHaptics(haptic);
#endif
    }
}
