using UnityEngine;

public class SettingsController : MonoBehaviour
{
    [SerializeField] GameObject panelVideo;
    [SerializeField] GameObject panelAudio;

    void OnEnable()
    {
        ShowVideo(); // por defecto al abrir Configuración
    }

    public void ShowVideo()
    {
        if (panelVideo) panelVideo.SetActive(true);
        if (panelAudio) panelAudio.SetActive(false);
    }

    public void ShowAudio()
    {
        if (panelVideo) panelVideo.SetActive(false);
        if (panelAudio) panelAudio.SetActive(true);
    }
}
