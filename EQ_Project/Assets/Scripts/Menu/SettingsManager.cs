using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string K_Fullscreen = "SET_fullscreen";
    private const string K_Mute = "SET_mute";
    private const string K_MasterVolume = "SET_masterVolume";

    [Header("Audio")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string masterVolumeParam = "MasterVolume";

    public bool Fullscreen { get; private set; }
    public bool Mute { get; private set; }
    public float Volume { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        ApplyAll();
    }

    public void Load()
    {
        Fullscreen = PlayerPrefs.GetInt(K_Fullscreen, Screen.fullScreen ? 1 : 0) == 1;
        Mute = PlayerPrefs.GetInt(K_Mute, 0) == 1;
        Volume = Mathf.Clamp01(PlayerPrefs.GetFloat(K_MasterVolume, 1f));
    }

    public void ApplyAll()
    {
        Screen.fullScreen = Fullscreen;
        ApplyAudio();
    }

    private void ApplyAudio()
    {
        if (mainMixer == null) return;

        if (Mute || Volume <= 0.0001f)
        {
            mainMixer.SetFloat(masterVolumeParam, -80f);
            return;
        }

        float db = Mathf.Log10(Volume) * 20f;
        mainMixer.SetFloat(masterVolumeParam, db);
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
        ApplyAudio();
        PlayerPrefs.SetInt(K_Mute, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVolume(float v)
    {
        Volume = Mathf.Clamp01(v);
        ApplyAudio();
        PlayerPrefs.SetFloat(K_MasterVolume, Volume);
        PlayerPrefs.Save();
    }
}