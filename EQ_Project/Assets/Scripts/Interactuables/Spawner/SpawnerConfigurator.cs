using CB.Balance;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerConfigurator : MonoBehaviour
{
    [SerializeField] SimpleSpawnerInteractable[] spawners;
    [SerializeField] InventoryDatabase inventory;

    public void ConfigureForReaction(ReactionAsset reaction)
    {
        Debug.Log("[SpawnerConfigurator] ConfigureForReaction llamado");

        if (reaction == null)
        {
            Debug.LogError("[SpawnerConfigurator] Reaction es NULL");
            return;
        }

        if (inventory == null)
        {
            Debug.LogError("[SpawnerConfigurator] InventoryDatabase es NULL");
            return;
        }

        Debug.Log($"[SpawnerConfigurator] Reacción: {reaction.name}");
        Debug.Log($"[SpawnerConfigurator] Spawners disponibles: {spawners.Length}");

        List<ItemDefinition> required = new();

        Collect(reaction.lhs, required);
        Collect(reaction.rhs, required);

        Debug.Log($"[SpawnerConfigurator] Elementos requeridos únicos: {required.Count}");

        for (int i = 0; i < spawners.Length; i++)
        {
            var spawner = spawners[i];

            if (spawner == null)
            {
                Debug.LogWarning($"[SpawnerConfigurator] Spawner {i} es NULL");
                continue;
            }

            if (i < required.Count)
            {
                Debug.Log($"[SpawnerConfigurator] Spawner {i} configurado con item: {required[i].id}");
                spawner.Configure(required[i]);
            }
            else
            {
                Debug.Log($"[SpawnerConfigurator] Spawner {i} DESACTIVADO (no requerido)");
                spawner.gameObject.SetActive(false);
            }
        }
    }

    void Collect(string[] species, List<ItemDefinition> list)
    {
        if (species == null)
        {
            Debug.LogWarning("[SpawnerConfigurator] Species array es NULL");
            return;
        }

        foreach (var s in species)
        {
            Debug.Log($"[SpawnerConfigurator] Analizando especie: {s}");

            var parsed = ChemFormula.Parse(s);

            foreach (var elem in parsed.Keys)
            {
                Debug.Log($"[SpawnerConfigurator] Elemento detectado: {elem}");

                if (list.Exists(i => i.id == elem))
                {
                    Debug.Log($"[SpawnerConfigurator] Ya agregado, se omite");
                    continue;
                }

                var item = inventory.FindById(elem);

                if (item == null)
                {
                    Debug.LogError($"[SpawnerConfigurator] ItemDefinition NO encontrado en InventoryDatabase para id: {elem}");
                    continue;
                }

                Debug.Log($"[SpawnerConfigurator] ItemDefinition encontrado: {item.name}");
                list.Add(item);
            }
        }
    }
}
