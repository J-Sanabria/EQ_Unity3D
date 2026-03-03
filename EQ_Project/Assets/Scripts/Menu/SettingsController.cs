using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Toggle toggleFullscreen;
    [SerializeField] Toggle toggleMute;
    [SerializeField] Slider sliderVolume;

    void OnEnable()
    {
        var sm = SettingsManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[SettingsController] No existe SettingsManager en escena.");
            return;
        }

        // Set UI sin disparar eventos
        if (toggleFullscreen) toggleFullscreen.SetIsOnWithoutNotify(sm.Fullscreen);
        if (toggleMute) toggleMute.SetIsOnWithoutNotify(sm.Mute);
        if (sliderVolume) sliderVolume.SetValueWithoutNotify(sm.Volume);

        // Hooks
        if (toggleFullscreen)
        {
            toggleFullscreen.onValueChanged.RemoveAllListeners();
            toggleFullscreen.onValueChanged.AddListener(sm.SetFullscreen);
        }

        if (toggleMute)
        {
            toggleMute.onValueChanged.RemoveAllListeners();
            toggleMute.onValueChanged.AddListener(sm.SetMute);
        }

        if (sliderVolume)
        {
            sliderVolume.onValueChanged.RemoveAllListeners();
            sliderVolume.onValueChanged.AddListener(sm.SetVolume);
        }
    }
}