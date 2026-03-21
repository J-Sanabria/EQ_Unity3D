using UnityEngine;

[CreateAssetMenu(menuName = "ChemicalBalance/AlertStep", fileName = "ALT_")]
public class AlertStepAsset : ScriptableObject
{
    [Header("Contenido")]
    public string stepId = "alert_01";
    public string speakerName = "Sistema";
    public Sprite portrait;

    [TextArea(2, 6)]
    public string text;

    public string hint = "";

    [Header("Timing")]
    [Min(0.25f)] public float duration = 1.75f;
    [Min(0f)] public float delayBeforeShow = 0f;
    public bool useUnscaledTime = true;
}