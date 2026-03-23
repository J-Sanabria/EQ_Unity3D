using UnityEngine;

[CreateAssetMenu(menuName = "ChemicalBalance/Reaction", fileName = "RXN_")]
public class ReactionAsset : ScriptableObject
{
    [Header("Identidad")]
    public string reactionId = "rxn_001";
    [TextArea] public string title;

    [Header("Especies")]
    public string[] lhs = { "H2", "O2" };
    public string[] rhs = { "H2O" };

    [Header("Coeficientes iniciales (muestran en HUD)")]
    public int[] coefL = { 2, 1 };
    public int[] coefR = { 2 };

    [Header("Score Design")]
    [Min(0)] public int idealSteps = 4;
    [Min(0)] public int tutorialFixedScore = 1000;

    [Header("Recommended Balance Time (seconds)")]
    [Min(0)] public float easyTargetTime = 120f;
    [Min(0)] public float mediumTargetTime = 90f;
    [Min(0)] public float hardTargetTime = 60f;

    void OnValidate()
    {
        if (lhs != null && coefL != null && coefL.Length != lhs.Length)
            Debug.LogWarning($"[{name}] coefL.Length != lhs.Length");

        if (rhs != null && coefR != null && coefR.Length != rhs.Length)
            Debug.LogWarning($"[{name}] coefR.Length != rhs.Length");
    }
}