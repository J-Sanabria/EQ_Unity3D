using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyRing : MonoBehaviour, IKeyReceiver
{
    private readonly HashSet<PhaseKey> _keys = new();

    public bool HasKey(PhaseKey key) => _keys.Contains(key);

    public bool ReceiveKey(PhaseKey key, Transform source)
    {
        // si ya la tenía, no hacer nada
        if (_keys.Contains(key)) return false;

        _keys.Add(key);
        // aquí puedes disparar eventos para UI / manager
        return true;
    }

    public void ClearKeys() => _keys.Clear();
}