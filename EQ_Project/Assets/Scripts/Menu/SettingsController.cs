using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Toggle toggleFullscreen;
    [SerializeField] Toggle toggleMute;
    [SerializeField] Slider sliderVolume; // 0..1

    const string K_Fullscreen = "SET_fullscreen";
    const string K_Mute = "SET_mute";
    const string K_Volume = "SET_volume";

    void Start()
    {
        // Cargar
        bool fs = PlayerPrefs.GetInt(K_Fullscreen, Screen.fullScreen ? 1 : 0) == 1;
        bool mute = PlayerPrefs.GetInt(K_Mute, 0) == 1;
        float vol = PlayerPrefs.GetFloat(K_Volume, 1f);

        ApplyFullscreen(fs, save: false);
        ApplyMute(mute, save: false);
        ApplyVolume(vol, save: false);

        // UI init
        if (toggleFullscreen) toggleFullscreen.isOn = fs;
        if (toggleMute) toggleMute.isOn = mute;
        if (sliderVolume) sliderVolume.value = vol;

        // Hooks
        if (toggleFullscreen) toggleFullscreen.onValueChanged.AddListener(v => ApplyFullscreen(v, save: true));
        if (toggleMute) toggleMute.onValueChanged.AddListener(v => ApplyMute(v, save: true));
        if (sliderVolume) sliderVolume.onValueChanged.AddListener(v => ApplyVolume(v, save: true));
    }

    void ApplyFullscreen(bool on, bool save)
    {
        Screen.fullScreen = on;
        if (save) PlayerPrefs.SetInt(K_Fullscreen, on ? 1 : 0);
    }

    void ApplyMute(bool on, bool save)
    {
        AudioListener.pause = on; // pausa todo el audio
        if (save) PlayerPrefs.SetInt(K_Mute, on ? 1 : 0);
    }

    void ApplyVolume(float v, bool save)
    {
        v = Mathf.Clamp01(v);
        AudioListener.volume = v;
        if (save) PlayerPrefs.SetFloat(K_Volume, v);
    }
}