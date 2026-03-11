using System.Collections.Generic;
using UnityEngine;

public class PhaseKeyActivator : MonoBehaviour
{
    [System.Serializable]
    public struct KeyRef
    {
        public PhaseKey key;
        public GameObject keyObject;
        public PhaseKeyCollectible collectible;
    }

    [SerializeField] private List<KeyRef> keys = new();
    [SerializeField] private MinimapController minimapController;

    public void SetActiveKeys(HashSet<PhaseKey> activeKeys)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            KeyRef keyRef = keys[i];
            bool shouldBeActive = activeKeys.Contains(keyRef.key);

            GameObject targetObject = keyRef.keyObject != null
                ? keyRef.keyObject
                : (keyRef.collectible != null ? keyRef.collectible.gameObject : null);

            if (targetObject == null)
                continue;

            targetObject.SetActive(shouldBeActive);

            if (shouldBeActive && keyRef.collectible != null)
                keyRef.collectible.ResetKey();
        }

        if (minimapController != null)
            minimapController.RebuildTargets();
    }
}