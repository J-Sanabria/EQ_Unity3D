using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Toggle toggleFullscreen;
    [SerializeField] private Toggle toggleMute;
    [SerializeField] private Slider sliderVolume;

    private SettingsManager _settings;
    private Coroutine _bindRoutine;

    private void OnEnable()
    {
        _bindRoutine = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }

        UnbindUI();
    }

    private IEnumerator BindWhenReady()
    {
        UnbindUI();

        int tries = 0;
        while (SettingsManager.Instance == null && tries < 30)
        {
            tries++;
            yield return null;
        }

        _settings = SettingsManager.Instance;

        if (_settings == null)
        {
            Debug.LogError("[SettingsController] No existe SettingsManager en escena.");
            yield break;
        }

        BindUI();
        RefreshUI();

        Debug.Log("[SettingsController] Conectado a SettingsManager correctamente.", this);
    }

    private void BindUI()
    {
        if (toggleFullscreen != null)
            toggleFullscreen.onValueChanged.AddListener(OnFullscreenChanged);

        if (toggleMute != null)
            toggleMute.onValueChanged.AddListener(OnMuteChanged);

        if (sliderVolume != null)
            sliderVolume.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void UnbindUI()
    {
        if (toggleFullscreen != null)
            toggleFullscreen.onValueChanged.RemoveListener(OnFullscreenChanged);

        if (toggleMute != null)
            toggleMute.onValueChanged.RemoveListener(OnMuteChanged);

        if (sliderVolume != null)
            sliderVolume.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    public void RefreshUI()
    {
        if (_settings == null) return;

        if (toggleFullscreen != null)
            toggleFullscreen.SetIsOnWithoutNotify(_settings.Fullscreen);

        if (toggleMute != null)
            toggleMute.SetIsOnWithoutNotify(_settings.Mute);

        if (sliderVolume != null)
            sliderVolume.SetValueWithoutNotify(_settings.Volume);
    }

    private void OnFullscreenChanged(bool value)
    {
        _settings?.SetFullscreen(value);
    }

    private void OnMuteChanged(bool value)
    {
        _settings?.SetMute(value);
    }

    private void OnVolumeChanged(float value)
    {
        _settings?.SetVolume(value);
    }

    public void ResetSettingsUI()
    {
        if (_settings == null) return;

        _settings.ResetToDefaults();
        RefreshUI();
    }

    public void ClearSavedSettingsUI()
    {
        if (_settings == null) return;

        _settings.ClearSavedSettingsAndReload();
        RefreshUI();
    }
}