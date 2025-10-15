using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public ItemDefinition item;
    public int count;
}

public class PlayerInventory : MonoBehaviour, ICollector
{
    [Header("Config")]
    [SerializeField] public InventoryDatabase database;
    [SerializeField] public int slots = 9;

    [Header("Estado")]
    public List<InventorySlot> data;

    public event Action OnChanged;

    void Awake() => EnsureInit();
    void OnValidate() => EnsureInit();

    void EnsureInit()
    {
        if (slots <= 0) slots = 9;
        if (data == null || data.Count != slots)
        {
            data = new List<InventorySlot>(slots);
            for (int i = 0; i < slots; i++) data.Add(new InventorySlot());
        }
    }

    public bool Collect(string itemId, int amount, Transform source)
    {
        if (database == null) { Debug.LogWarning("PlayerInventory: database no asignada."); return false; }
        var def = database.FindById(itemId);
        if (def == null) { Debug.LogWarning("PlayerInventory: item no encontrado en DB: " + itemId); return false; }
        return Add(def, amount);
    }

    public bool Add(ItemDefinition item, int amount)
    {
        if (item == null || amount <= 0) return false;
        int remaining = amount;

        for (int i = 0; i < data.Count; i++)
        {
            var s = data[i];
            if (s.item == item && s.count < item.maxStack)
            {
                int canAdd = Mathf.Min(item.maxStack - s.count, remaining);
                s.count += canAdd;
                remaining -= canAdd;
                if (remaining <= 0) { OnChanged?.Invoke(); Debug.Log("Add OK: " + item.id + " x" + amount); return true; }
            }
        }
        for (int i = 0; i < data.Count; i++)
        {
            var s = data[i];
            if (s.item == null)
            {
                int put = Mathf.Min(item.maxStack, remaining);
                s.item = item; s.count = put;
                remaining -= put;
                if (remaining <= 0) { OnChanged?.Invoke(); Debug.Log("Add OK (slot nuevo): " + item.id + " x" + amount); return true; }
            }
        }
        OnChanged?.Invoke();
        Debug.LogWarning("Add parcial: quedo remanente " + remaining);
        return remaining <= 0;
    }

    public bool Remove(ItemDefinition item, int amount)
    {
        if (item == null || amount <= 0) return false;
        int remaining = amount;

        for (int i = 0; i < data.Count; i++)
        {
            var s = data[i];
            if (s.item == item && s.count > 0)
            {
                int take = Mathf.Min(s.count, remaining);
                s.count -= take;
                remaining -= take;
                if (s.count <= 0) s.item = null;
                if (remaining <= 0) { OnChanged?.Invoke(); return true; }
            }
        }
        OnChanged?.Invoke();
        return remaining <= 0;
    }

    public int CountOf(ItemDefinition item)
    {
        if (item == null) return 0;
        int total = 0;
        for (int i = 0; i < data.Count; i++)
            if (data[i].item == item) total += data[i].count;
        return total;
    }
}
