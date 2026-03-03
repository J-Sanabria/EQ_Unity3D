using UnityEngine;

public enum TutorialAdvanceMode
{
    PressContinue,
    WaitForFlag
}



[CreateAssetMenu(menuName = "ChemicalBalance/TutorialStep", fileName = "TUT_")]
public class TutorialStepAsset : ScriptableObject
{
    [Header("Narrativa")]
    public string stepId = "intro_01";
    public string speakerName = "Dalton";
    public Sprite portrait;
    public bool autoCloseByTime = false;
    public float autoCloseSeconds = 6f; // 0..10 recomendado

    [Header("Timing")]
    [Min(0f)] public float delayBeforeShow = 0f;
    public bool delayUsesUnscaledTime = true;
    public bool pauseWhileShowing = true;

    [TextArea(3, 10)] public string text;
    public string hint = "Enter - Continuar | Espacio - Saltar texto";

    [Header("Avance")]
    public TutorialAdvanceMode advanceMode = TutorialAdvanceMode.PressContinue;

    // Si advanceMode = WaitForFlag
    public string requiredFlag = "";
}