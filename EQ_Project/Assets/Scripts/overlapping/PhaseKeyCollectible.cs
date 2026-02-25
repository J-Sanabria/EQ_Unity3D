using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum PhaseKey
{
    Metals,
    NonMetals,
    Hydrogen,
    Oxygen
}

public interface IKeyReceiver
{
    /// <returns>true si la llave fue aceptada</returns>
    bool ReceiveKey(PhaseKey key, Transform source);
}

[RequireComponent(typeof(Collider))]
public class PhaseKeyCollectible : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private PhaseKey key;

    [Header("Detección")]
    [SerializeField] private LayerMask collectorLayers;
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool onlyOnce = true;

    [Header("Feedback")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private ParticleSystem pickupVfx;
    [SerializeField] private Renderer[] renderersToHide;
    [SerializeField] private Collider triggerCollider;

    [Header("Respawn")]
    [SerializeField] private bool respawn;
    [SerializeField] private float respawnSeconds = 10f;

    [Header("Eventos")]
    [SerializeField] private UnityEvent onCollected;

    private bool _available = true;
    private AudioSource _audio;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        triggerCollider = col;

        var r = GetComponentInChildren<Renderer>();
        if (r != null) renderersToHide = new Renderer[] { r };
    }

    void Awake()
    {
        if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null) triggerCollider.isTrigger = true;

        _audio = GetComponent<AudioSource>();
        if (_audio == null && pickupSfx != null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 1f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_available) return;

        if (((1 << other.gameObject.layer) & collectorLayers.value) == 0) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        TryCollect(other.transform);
    }

    void TryCollect(Transform collector)
    {
        var receiver = collector.GetComponentInParent<IKeyReceiver>();
        if (receiver == null) return;

        bool accepted = receiver.ReceiveKey(key, transform);
        if (!accepted) return;

        // Feedback
        if (pickupVfx != null) Instantiate(pickupVfx, transform.position, Quaternion.identity);
        if (_audio != null && pickupSfx != null) _audio.PlayOneShot(pickupSfx);

        onCollected?.Invoke();

        if (respawn) StartCoroutine(DoRespawn());
        else if (onlyOnce) Destroy(gameObject);
        else
        {
            SetVisible(false);
            _available = false;
        }
    }

    IEnumerator DoRespawn()
    {
        _available = false;
        SetVisible(false);
        if (triggerCollider != null) triggerCollider.enabled = false;

        yield return new WaitForSeconds(Mathf.Max(0f, respawnSeconds));

        if (triggerCollider != null) triggerCollider.enabled = true;
        SetVisible(true);
        _available = true;
    }

    void SetVisible(bool v)
    {
        if (renderersToHide == null) return;
        for (int i = 0; i < renderersToHide.Length; i++)
            if (renderersToHide[i] != null)
                renderersToHide[i].enabled = v;
    }
}
