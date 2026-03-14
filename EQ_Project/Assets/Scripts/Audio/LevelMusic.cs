using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    private void Start()
    {
        if (!playOnStart) return;
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.PlayMusic(musicClip, loop, volume);
    }
}