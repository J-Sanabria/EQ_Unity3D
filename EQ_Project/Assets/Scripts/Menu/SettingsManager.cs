using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    const string K_Fullscreen = "SET_fullscreen";
    const string K_Mute = "SET_mute";
    const string K_Volume = "SET_volume";

    public bool Fullscreen { get; private set; }
    public bool Mute { get; private set; }
    public float Volume { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        ApplyAll();
    }

    public void Load()
    {
        Fullscreen = PlayerPrefs.GetInt(K_Fullscreen, Screen.fullScreen ? 1 : 0) == 1;
        Mute = PlayerPrefs.GetInt(K_Mute, 0) == 1;
        Volume = PlayerPrefs.GetFloat(K_Volume, 1f);
        Volume = Mathf.Clamp01(Volume);
    }

    public void ApplyAll()
    {
        Screen.fullScreen = Fullscreen;
        AudioListener.pause = Mute;
        AudioListener.volume = Volume;
    }

    public void SetFullscreen(bool on)
    {
        Fullscreen = on;
        Screen.fullScreen = on;
        PlayerPrefs.SetInt(K_Fullscreen, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMute(bool on)
    {
        Mute = on;
        AudioListener.pause = on;
        PlayerPrefs.SetInt(K_Mute, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVolume(float v)
    {
        Volume = Mathf.Clamp01(v);
        AudioListener.volume = Volume;
        PlayerPrefs.SetFloat(K_Volume, Volume);
        PlayerPrefs.Save();
    }
}