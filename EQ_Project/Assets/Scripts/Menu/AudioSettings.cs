using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] Slider slMaster;

    const string PF_VOL = "aud_master";

    void Awake()
    {
        float vol = PlayerPrefs.GetFloat(PF_VOL, 1f);
        if (slMaster)
        {
            slMaster.minValue = 0f;
            slMaster.maxValue = 1f;
            slMaster.value = vol;
            slMaster.onValueChanged.AddListener(SetMasterVolume);
        }
        AudioListener.volume = vol;
    }

    void SetMasterVolume(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(PF_VOL, v);
        PlayerPrefs.Save();
    }
}
