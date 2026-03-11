using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("2D SFX")]
    [SerializeField] private AudioSource sfx2DSource;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("3D SFX")]
    [SerializeField] private AudioSource sfx3DPrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfx2DSource == null)
        {
            GameObject go = new GameObject("SFX_2D");
            go.transform.SetParent(transform);
            sfx2DSource = go.AddComponent<AudioSource>();
            sfx2DSource.playOnAwake = false;
            sfx2DSource.spatialBlend = 0f;
        }

        if (musicSource == null)
        {
            GameObject go = new GameObject("Music");
            go.transform.SetParent(transform);
            musicSource = go.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
        }
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfx2DSource == null) return;
        sfx2DSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void PlaySfxAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float spatialBlend = 1f)
    {
        if (clip == null) return;

        if (sfx3DPrefab != null)
        {
            AudioSource src = Instantiate(sfx3DPrefab, position, Quaternion.identity);
            src.spatialBlend = spatialBlend;
            src.clip = clip;
            src.volume = Mathf.Clamp01(volume);
            src.Play();
            Destroy(src.gameObject, clip.length + 0.1f);
            return;
        }

        GameObject go = new GameObject("Temp3DSFX");
        go.transform.position = position;

        AudioSource srcFallback = go.AddComponent<AudioSource>();
        srcFallback.playOnAwake = false;
        srcFallback.spatialBlend = spatialBlend;
        srcFallback.clip = clip;
        srcFallback.volume = Mathf.Clamp01(volume);
        srcFallback.Play();

        Destroy(go, clip.length + 0.1f);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
    {
        if (musicSource == null) return;
        if (musicSource.clip == clip) return;

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