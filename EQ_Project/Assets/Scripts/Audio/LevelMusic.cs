using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private bool playOnStart = true;

    private void Start()
    {
        if (!playOnStart) return;
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.PlayMusic(musicClip, true, musicVolume);
    }
}