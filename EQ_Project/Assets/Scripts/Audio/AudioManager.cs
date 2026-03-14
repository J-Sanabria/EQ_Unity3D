using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ValidateAndConfigureSources();
    }

    private void ValidateAndConfigureSources()
    {
        if (sfxSource == null)
            Debug.LogError("[AudioManager] Falta asignar SFX Source.", this);

        if (musicSource == null)
            Debug.LogError("[AudioManager] Falta asignar Music Source.", this);

        if (sfxGroup == null)
            Debug.LogError("[AudioManager] Falta asignar SFX Group.", this);

        if (musicGroup == null)
            Debug.LogError("[AudioManager] Falta asignar Music Group.", this);

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.outputAudioMixerGroup = sfxGroup;
        }

        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.outputAudioMixerGroup = musicGroup;
        }
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
    {
        if (musicSource == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = Mathf.Clamp01(volume);

        if (clip != null)
            musicSource.Play();
        else
            musicSource.Stop();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}