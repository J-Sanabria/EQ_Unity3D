using UnityEngine;

public class MinimapTarget : MonoBehaviour
{
    public enum TargetType
    {
        Key
    }

    [Header("Config")]
    public TargetType type = TargetType.Key;
    public PhaseKey phaseKey;

    public bool IsAvailable()
    {
        return isActiveAndEnabled && gameObject.activeInHierarchy;
    }
}