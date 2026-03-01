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
            var c = keys[i].collectible;
            if (c == null) continue;

            bool shouldBeActive = activeKeys.Contains(keys[i].key);

            if (shouldBeActive)
                c.ResetKey();
            else
                c.gameObject.SetActive(false);
        }
    }
}