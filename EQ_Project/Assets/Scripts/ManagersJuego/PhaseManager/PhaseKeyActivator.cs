using System.Collections.Generic;
using UnityEngine;
using CB.Balance;

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

    public void SetActiveKeys(HashSet<PhaseKey> activeKeys)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            var obj = keys[i].keyObject;
            if (obj == null) continue;

            bool shouldBeActive = activeKeys.Contains(keys[i].key);

            if (shouldBeActive)
            {
                // reset físico si existe collectible
                if (keys[i].collectible != null)
                    keys[i].collectible.ResetKey();
            }

            obj.SetActive(shouldBeActive);
        }
    }
}