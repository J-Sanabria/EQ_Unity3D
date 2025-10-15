using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [Header("Datos del objeto")]
    public string itemId = "element_H";   // identificador lógico
    public string displayName = "Hidrogeno";
    public int amount = 1;

    [Header("Deteccion")]
    public LayerMask collectorLayers;     // capas que pueden recoger (ej: Player)
    public string requiredTag = "Player"; // opcional; vacio para ignorar
    public bool onlyOnce = true;          // si true, se destruye/oculta tras recoger

    [Header("Feedback")]
    public AudioClip pickupSfx;
    public ParticleSystem pickupVfx;
    public Renderer[] renderersToHide;    // si se desactiva temporalmente
    public Collider triggerCollider;      // si no se asigna, usa el propio

    [Header("Respawn")]
    public bool respawn;
    public float respawnSeconds = 10f;    // tiempo de reaparicion si respawn=true

    [Header("Eventos")]
    public UnityEvent onCollected;        // para hooks en el editor

    bool _available = true;
    AudioSource _audio;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        triggerCollider = col;
        // Intenta autoasignar renderer
        var r = GetComponentInChildren<Renderer>();
        if (r != null) renderersToHide = new Renderer[] { r };
    }

    void Awake()
    {
        if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null) triggerCollider.isTrigger = true;

        // AudioSource opcional local para SFX 3D
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

        // Filtro por capas
        if (((1 << other.gameObject.layer) & collectorLayers.value) == 0) return;

        // Filtro por tag (si se definio)
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        TryCollect(other.transform);
    }

    void TryCollect(Transform collector)
    {
        // Si el collector implementa inventario, pregúntale si acepta
        var ic = collector.GetComponentInParent<ICollector>();
        bool accepted = true;
        if (ic != null)
            accepted = ic.Collect(itemId, amount, transform);

        if (!accepted) return;

        // Feedback
        if (pickupVfx != null) Instantiate(pickupVfx, transform.position, Quaternion.identity);
        if (_audio != null && pickupSfx != null) _audio.PlayOneShot(pickupSfx);

        onCollected?.Invoke();

        if (respawn)
        {
            StartCoroutine(DoRespawn());
        }
        else if (onlyOnce)
        {
            Destroy(gameObject);
        }
        else
        {
            // Ocultar sin respawn
            SetVisible(false);
            _available = false;
        }
    }

    IEnumerator DoRespawn()
    {
        _available = false;
        SetVisible(false);
        if (triggerCollider != null) triggerCollider.enabled = false;

        float t = Mathf.Max(0f, respawnSeconds);
        yield return new WaitForSeconds(t);

        if (triggerCollider != null) triggerCollider.enabled = true;
        SetVisible(true);
        _available = true;
    }

    void SetVisible(bool v)
    {
        if (renderersToHide != null)
        {
            for (int i = 0; i < renderersToHide.Length; i++)
            {
                if (renderersToHide[i] != null)
                    renderersToHide[i].enabled = v;
            }
        }
    }
}
