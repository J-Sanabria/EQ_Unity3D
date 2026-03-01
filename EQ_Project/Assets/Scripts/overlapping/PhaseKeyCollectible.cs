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

    [Header("Detection")]
    [SerializeField] private LayerMask collectorLayers;
    [SerializeField] private string requiredTag = "Player";

    [Header("Feedback (optional)")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private ParticleSystem pickupVfx;

    Collider _col;
    AudioSource _audio;

    public PhaseKey Key => key;

    void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;

        // AudioSource opcional
        if (pickupSfx != null)
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null)
            {
                _audio = gameObject.AddComponent<AudioSource>();
                _audio.playOnAwake = false;
                _audio.spatialBlend = 1f;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Layer filter
        if (((1 << other.gameObject.layer) & collectorLayers.value) == 0) return;

        // Tag filter (si lo usas)
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        var receiver = other.GetComponentInParent<IKeyReceiver>();
        if (receiver == null) return;

        if (!receiver.ReceiveKey(key, transform)) return;

        if (pickupVfx != null)
            Instantiate(pickupVfx, transform.position, Quaternion.identity);

        if (_audio != null && pickupSfx != null)
            _audio.PlayOneShot(pickupSfx);

        // Desactivar COMPLETO: más simple que apagar render
        gameObject.SetActive(false);
    }

    public void ResetKey()
    {
        gameObject.SetActive(true);
    }
}