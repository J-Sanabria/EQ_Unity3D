using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string K_FULLSCREEN = "SET_fullscreen";
    private const string K_MUTE = "SET_mute";
    private const string K_MASTER_VOLUME = "SET_masterVolume";

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string masterVolumeParam = "MasterVolume";

    [Header("DB Range")]
    [SerializeField] private float minDb = -80f;
    [SerializeField] private float maxDb = 0f;

    [Header("Defaults")]
    [SerializeField] private bool defaultFullscreen = true;
    [SerializeField] private bool useExclusiveFullscreen = false;
    [SerializeField] private bool defaultMute = false;
    [SerializeField][Range(0f, 1f)] private float defaultVolume = 1f;

    public bool Fullscreen { get; private set; }
    public bool Mute { get; private set; }
    public float Volume { get; private set; }

    private void Awake()
    {
        // SOLO para pruebas. Quitar luego.
        // PlayerPrefs.DeleteAll();
        // PlayerPrefs.Save();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        ApplyAll();

        Debug.Log($"[SettingsManager] Init -> Fullscreen:{Fullscreen} Mute:{Mute} Volume:{Volume}", this);
    }

    public void Load()
    {
        Fullscreen = PlayerPrefs.GetInt(K_FULLSCREEN, defaultFullscreen ? 1 : 0) == 1;
        Mute = PlayerPrefs.GetInt(K_MUTE, defaultMute ? 1 : 0) == 1;
        Volume = Mathf.Clamp01(PlayerPrefs.GetFloat(K_MASTER_VOLUME, defaultVolume));
    }

    public void ApplyAll()
    {
        ApplyFullscreen();
        ApplyAudio();
    }

    private void ApplyFullscreen()
    {
        FullScreenMode mode = Fullscreen
            ? (useExclusiveFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.FullScreenWindow)
            : FullScreenMode.Windowed;

        Screen.fullScreenMode = mode;
        Screen.fullScreen = Fullscreen;

        Debug.Log($"[SettingsManager] ApplyFullscreen -> {Fullscreen} | Mode: {mode}", this);
    }

    private void ApplyAudio()
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("[SettingsManager] No hay AudioMixer asignado.", this);
            return;
        }

        float db = (Mute || Volume <= 0.0001f)
            ? minDb
            : Mathf.Lerp(minDb, maxDb, Volume);

        bool ok = mainMixer.SetFloat(masterVolumeParam, db);

        if (ok)
        {
            mainMixer.GetFloat(masterVolumeParam, out float currentDb);
            Debug.Log($"[SettingsManager] ApplyAudio -> Mute:{Mute} Volume:{Volume} dB:{currentDb}", this);
        }
        else
        {
            Debug.LogWarning(
                $"[SettingsManager] No se pudo aplicar '{masterVolumeParam}'. Revisa que el parámetro esté expuesto exactamente con ese nombre.",
                this
            );
        }
    }

    public void SetFullscreen(bool on)
    {
        Fullscreen = on;
        ApplyFullscreen();

        PlayerPrefs.SetInt(K_FULLSCREEN, on ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[SettingsManager] SetFullscreen -> {on}", this);
    }

    public void SetMute(bool on)
    {
        Mute = on;
        ApplyAudio();

        PlayerPrefs.SetInt(K_MUTE, on ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[SettingsManager] SetMute -> {on}", this);
    }

    public void SetVolume(float value)
    {
        Volume = Mathf.Clamp01(value);
        ApplyAudio();

        PlayerPrefs.SetFloat(K_MASTER_VOLUME, Volume);
        PlayerPrefs.Save();

        Debug.Log($"[SettingsManager] SetVolume -> {Volume}", this);
    }

    public void ResetToDefaults()
    {
        Fullscreen = defaultFullscreen;
        Mute = defaultMute;
        Volume = defaultVolume;

        ApplyAll();
        SaveCurrentState();

        Debug.Log("[SettingsManager] ResetToDefaults", this);
    }

    public void ClearSavedSettingsAndReload()
    {
        PlayerPrefs.DeleteKey(K_FULLSCREEN);
        PlayerPrefs.DeleteKey(K_MUTE);
        PlayerPrefs.DeleteKey(K_MASTER_VOLUME);
        PlayerPrefs.Save();

        Load();
        ApplyAll();

        Debug.Log("[SettingsManager] ClearSavedSettingsAndReload", this);
    }

    private void SaveCurrentState()
    {
        PlayerPrefs.SetInt(K_FULLSCREEN, Fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(K_MUTE, Mute ? 1 : 0);
        PlayerPrefs.SetFloat(K_MASTER_VOLUME, Volume);
        PlayerPrefs.Save();
    }
}