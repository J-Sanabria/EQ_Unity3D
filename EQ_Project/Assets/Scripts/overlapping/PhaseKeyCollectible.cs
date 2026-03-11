using UnityEngine;

public enum PhaseKey
{
    Metals,
    NonMetals,
    Hydrogen,
    Oxygen
}

public interface IKeyReceiver
{
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

    [Header("Feedback")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private float pickupSfxVolume = 1f;
    [SerializeField] private ParticleSystem pickupVfx;

    [SerializeField] private TutorialManager tutorial;

    private Collider _col;

    public PhaseKey Key => key;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & collectorLayers.value) == 0) return;

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        var receiver = other.GetComponentInParent<IKeyReceiver>();
        if (receiver == null) return;

        if (!receiver.ReceiveKey(key, transform))
            return;

        if (pickupVfx != null)
            Instantiate(pickupVfx, transform.position, Quaternion.identity);

        if (pickupSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfxAtPosition(pickupSfx, transform.position, pickupSfxVolume);

        tutorial?.PlayEventOnce(TutorialEvent.FirstKeyPicked);

        gameObject.SetActive(false);
    }

    public void ResetKey()
    {
        gameObject.SetActive(true);
    }
}