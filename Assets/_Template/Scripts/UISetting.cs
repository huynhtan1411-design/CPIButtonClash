using MoreMountains.NiceVibrations;
using TMPro;
using UISystems;
using UnityEngine;

public class UISetting : MonoBehaviour
{
    [Header(" Settings ")]
    public static bool state;
    public static bool stateMusic;
    [SerializeField] private UIToggleSwitch _toggleSwitchHaptic;
    [SerializeField] private UIToggleSwitch _toggleSwitchSound;
    [SerializeField] private UIToggleSwitch _toggleSwitchMusic;
    [SerializeField] private TMP_InputField _inputAmplitude;
    [SerializeField] private TMP_InputField _inputDuration;

    private void Awake()
    {
        UIManager.onSettingsSet = OpenSetting;
    }

    private void Start()
    {
        Audio_Manager.instance.GetData();
        Vibration_Manager.instance.GetData();
        _toggleSwitchSound.SetValue(!Audio_Manager.instance.soundMute);
        _toggleSwitchMusic.SetValue(!Audio_Manager.instance.musicMute);
        _toggleSwitchHaptic.SetValue(Vibration_Manager.instance.IsVibration);
        Init();
        //UpdateVibration(Vibration_Manager.instance.IsVibration);
        //UpdateSound(Audio_Manager.instance.soundMute);
        //UpdateMusic(Audio_Manager.instance.musicMute);

    }

    public void OpenSetting()
    {
        ShowVibration();
    }

    private void Init()
    {
        _toggleSwitchHaptic.OnChangeValue.AddListener(UpdateVibration);
        _toggleSwitchSound.OnChangeValue.AddListener(UpdateSound);
        _toggleSwitchMusic.OnChangeValue.AddListener(UpdateMusic);
    }

    private void UpdateVibration(bool value)
    {
        Vibration_Manager.instance.SetVibration(value);
    }
    private void UpdateSound(bool value)
    {
        Audio_Manager.instance.SwitchStateSound(!value);
    }
    private void UpdateMusic(bool value)
    {
        Audio_Manager.instance.SwitchStateMusic(!value);
    }

    public void ShowVibration()
    {
        _inputAmplitude.text = MMVibrationManager.LightAmplitude.ToString();
        _inputDuration.text = MMVibrationManager.LightDuration.ToString();
    }

    public void SaveVibration()
    {
        MMVibrationManager.LightAmplitude = int.Parse(_inputAmplitude.text);
        MMVibrationManager.LightDuration = int.Parse(_inputDuration.text); ;
    }
}
