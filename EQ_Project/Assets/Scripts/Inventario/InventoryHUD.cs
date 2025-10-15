using System.Collections.Generic;
using UnityEngine;

public class InventoryHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerInventory inventory;
    public Transform slotsParent;  // el Grid/Horizontal contenedor
    public GameObject slotPrefab;  // tu prefab UI_InvSlot

    private readonly List<InventorySlotUI> _slotsUI = new List<InventorySlotUI>();
    private bool _built;

    void Awake()
    {
        if (inventory == null) inventory = FindObjectOfType<PlayerInventory>();
    }

    void OnEnable()
    {
        BuildSlots();
        if (inventory != null) inventory.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    void BuildSlots()
    {
        if (inventory == null) { Debug.LogWarning("InventoryHUD: inventory no asignado"); return; }
        if (slotsParent == null) { Debug.LogWarning("InventoryHUD: slotsParent no asignado"); return; }
        if (slotPrefab == null) { Debug.LogWarning("InventoryHUD: slotPrefab no asignado"); return; }

        _slotsUI.Clear();
        for (int i = slotsParent.childCount - 1; i >= 0; i--)
            Destroy(slotsParent.GetChild(i).gameObject);

        for (int i = 0; i < inventory.data.Count; i++)
        {
            var go = Instantiate(slotPrefab, slotsParent);
            var ui = go.GetComponent<InventorySlotUI>();
            if (ui == null) Debug.LogWarning("slotPrefab no tiene InventorySlotUI.");
            _slotsUI.Add(ui);
        }
        _built = true;
        Debug.Log("InventoryHUD: slots construidos = " + _slotsUI.Count);
    }

    public void Refresh()
    {
        if (!_built) BuildSlots();
        if (inventory == null) return;

        int n = Mathf.Min(_slotsUI.Count, inventory.data.Count);
        for (int i = 0; i < n; i++)
        {
            var s = inventory.data[i];
            var ui = _slotsUI[i];
            if (ui == null) continue;

            var sprite = s.item ? s.item.icon : null;
            int count = s.item ? s.count : 0;
            ui.Set(sprite, count);
        }
    }
}
