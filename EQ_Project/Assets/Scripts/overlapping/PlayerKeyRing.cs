using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyRing : MonoBehaviour, IKeyReceiver
{
    [SerializeField] private PhaseManager phaseManager;

    private readonly HashSet<PhaseKey> _keys = new();

    void Reset()
    {
        if (phaseManager == null)
            phaseManager = Object.FindFirstObjectByType<PhaseManager>();
    }

    void Awake()
    {
        // fallback runtime por si Reset no corrió o quedó sin asignar
        if (phaseManager == null)
            phaseManager = Object.FindFirstObjectByType<PhaseManager>();
    }

    public bool HasKey(PhaseKey key) => _keys.Contains(key);

    public bool ReceiveKey(PhaseKey key, Transform source)
    {
        if (_keys.Contains(key)) return false;

        _keys.Add(key);

        if (phaseManager == null)
            phaseManager = Object.FindFirstObjectByType<PhaseManager>();

        if (phaseManager != null)
            phaseManager.ReceiveKey(key, source);
        else
            Debug.LogWarning("[PlayerKeyRing] PhaseManager no encontrado, la llave no desbloqueó fase.");

        return true;
    }

    public void ClearKeys() => _keys.Clear();
}