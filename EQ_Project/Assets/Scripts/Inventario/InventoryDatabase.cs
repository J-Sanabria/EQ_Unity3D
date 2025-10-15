using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ChemicalBalance/InventoryDatabase", fileName = "InventoryDatabase")]
public class InventoryDatabase : ScriptableObject
{
    public List<ItemDefinition> items = new List<ItemDefinition>();

    public ItemDefinition FindById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (it != null && it.id == id) return it;
        }
        return null;
    }
}
