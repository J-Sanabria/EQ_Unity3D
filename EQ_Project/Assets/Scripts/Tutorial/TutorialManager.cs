using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ScheduledBlock
{
    public TutorialBlockAsset block;
    [Min(0f)] public float delaySeconds = 0f;
    public bool useUnscaledTime = true; // recomendado si en intro pausas
}

public class TutorialManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] TutorialUI ui;

    [Header("Blocks")]
    [SerializeField] TutorialBlockAsset introBlock;
    [SerializeField] TutorialBlockAsset movementBlock;
    [SerializeField] TutorialBlockAsset keyPickedBlock;
    [SerializeField] TutorialBlockAsset enterBalanceBlock;
    [SerializeField] List<ScheduledBlock> startSequence = new();

    [Header("Behavior")]
    [SerializeField] bool pauseGameWhileDialogue = true;
    [SerializeField] bool allowQueue = false; // si false, no permite disparar bloque si ya hay uno activo

    readonly HashSet<string> _playedBlocks = new();
    Queue<TutorialStepAsset> _queue = new();

    TutorialStepAsset _current;
    bool _running;
    Coroutine _autoCloseRoutine;

    // flags opcionales
    readonly HashSet<string> _flags = new();

    void Start()
    {
        StartCoroutine(RunStartSequence());
    }

    IEnumerator RunStartSequence()
    {
        for (int i = 0; i < startSequence.Count; i++)
        {
            var s = startSequence[i];
            if (s == null || s.block == null) continue;

            float t = 0f;
            float d = Mathf.Max(0f, s.delaySeconds);

            while (t < d)
            {
                t += s.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            PlayBlockOnce(s.block);
        }
    }


    public void PlayBlockOnce(TutorialBlockAsset block)
    {
        if (block == null) return;
        if (_playedBlocks.Contains(block.blockId)) return;

        if (_running && !allowQueue) return;

        _playedBlocks.Add(block.blockId);

        // encola pasos
        for (int i = 0; i < block.steps.Count; i++)
            if (block.steps[i] != null) _queue.Enqueue(block.steps[i]);

        if (!_running)
            NextStep();
    }

    public void SetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        _flags.Add(flag);
        TryAdvanceIfWaitingFlag();
    }

    void Update()
    {
        if (!_running || ui == null || !ui.IsOpen) return;

        // Space = saltar typing
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            ui.SkipTyping();

        // Enter = continuar (solo si el paso lo permite)
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (ui.IsTyping) ui.SkipTyping();
            else TryContinue();
        }
    }

    void NextStep()
    {
        if (_queue.Count == 0)
        {
            EndDialogue();
            return;
        }

        _running = true;
        _current = _queue.Dequeue();

        if (_autoCloseRoutine != null) StopCoroutine(_autoCloseRoutine);
        _autoCloseRoutine = null;

        StartCoroutine(ShowStepWithDelay(_current));
    }

    IEnumerator ShowStepWithDelay(TutorialStepAsset step)
    {
        float d = Mathf.Max(0f, step.delayBeforeShow);
        float t = 0f;

        // Durante este delay, el jugador puede actuar
        while (t < d)
        {
            t += step.delayUsesUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        if (step.pauseWhileShowing)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;


        ui.Show(step.portrait, step.speakerName, step.text, step.hint);

        // AutoClose si aplica
        if (step.autoCloseByTime && step.autoCloseSeconds > 0f)
            _autoCloseRoutine = StartCoroutine(AutoCloseAfter(step.autoCloseSeconds));

        TryAdvanceIfWaitingFlag();
    }

    IEnumerator AutoCloseAfter(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // si aún está abierto, avanza como si fuera continue
        TryContinue(force: true);
    }

    void TryContinue(bool force = false)
    {
        if (_current == null) return;

        if (_current.advanceMode == TutorialAdvanceMode.PressContinue || force)
        {
            ui.Hide();
            if (pauseGameWhileDialogue) Time.timeScale = 1f;
            NextStep();
            return;
        }

        // WaitForFlag: Enter NO avanza (se queda esperando)
    }

    void TryAdvanceIfWaitingFlag()
    {
        if (_current == null) return;
        if (_current.advanceMode != TutorialAdvanceMode.WaitForFlag) return;

        if (!string.IsNullOrEmpty(_current.requiredFlag) && _flags.Contains(_current.requiredFlag))
        {
            ui.Hide();
            if (pauseGameWhileDialogue) Time.timeScale = 1f;
            NextStep();
        }
    }

    void EndDialogue()
    {
        _running = false;
        _current = null;
        ui.Hide();
        if (pauseGameWhileDialogue) Time.timeScale = 1f;
    }
}