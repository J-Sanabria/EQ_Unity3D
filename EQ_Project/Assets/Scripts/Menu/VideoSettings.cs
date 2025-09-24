using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoSettings : MonoBehaviour
{
    [SerializeField] Toggle tgFullscreen;
    [SerializeField] TMP_Dropdown ddQuality;

    const string PF_FULL = "vid_fullscreen";
    const string PF_QUAL = "vid_quality";

    Resolution[] _resolutions;

    void Awake()
    {
        // --- Fullscreen ---
        bool isFull = PlayerPrefs.GetInt(PF_FULL, Screen.fullScreen ? 1 : 0) == 1;
        if (tgFullscreen)
        {
            tgFullscreen.isOn = isFull;
            tgFullscreen.onValueChanged.AddListener(SetFullscreen);
        }
        Screen.fullScreen = isFull;

        // --- Calidad ---
        if (ddQuality)
        {
            ddQuality.ClearOptions();
            ddQuality.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            int q = Mathf.Clamp(PlayerPrefs.GetInt(PF_QUAL, QualitySettings.GetQualityLevel()), 0, QualitySettings.names.Length - 1);
            ddQuality.value = q;
            ddQuality.RefreshShownValue();
            ddQuality.onValueChanged.AddListener(SetQualityLevel);
            QualitySettings.SetQualityLevel(q, true);
        }

    }

    void SetFullscreen(bool isFull)
    {
        Screen.fullScreen = isFull;
        PlayerPrefs.SetInt(PF_FULL, isFull ? 1 : 0);
        PlayerPrefs.Save();
    }

    void SetQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt(PF_QUAL, index);
        PlayerPrefs.Save();
    }

}
