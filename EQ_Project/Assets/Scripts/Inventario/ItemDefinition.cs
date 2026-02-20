using UnityEngine;

[CreateAssetMenu(menuName = "ChemicalBalance/Item", fileName = "IT_")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identidad")]
    public string id;           // ej: "element_H"
    public string displayName;  // ej: "Hidrogeno"

    [Header("Spawn")]
    public Collectible collectiblePrefab;

    [Header("Visual")]
    public Sprite icon;

    [Header("Stack")]
    public int maxStack = 99;
}