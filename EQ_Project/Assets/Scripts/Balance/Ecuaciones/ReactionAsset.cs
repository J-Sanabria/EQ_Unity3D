using UnityEngine;

[CreateAssetMenu(menuName = "ChemicalBalance/Reaction", fileName = "RXN_")]
public class ReactionAsset : ScriptableObject
{
    [Header("Identidad")]
    public string reactionId = "rxn_001";
    [TextArea] public string title; // opcional: para UI

    [Header("Especies")]
    public string[] lhs = { "H2", "O2" };
    public string[] rhs = { "H2O" };

    [Header("Coeficientes iniciales (muestran en HUD)")]
    public int[] coefL = { 2, 1 };
    public int[] coefR = { 2 };
}
