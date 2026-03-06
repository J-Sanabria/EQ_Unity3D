using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ScheduledBlock
{
    public TutorialBlockAsset block;
    [Min(0f)] public float delaySeconds = 0f;
    public bool useUnscaledTime = true;
}

public enum TutorialEvent
{
    Intro,
    Movement,
    FirstInteractableSeen,
    FirstInteract,
    FirstKeyPicked,
    EnterBalance,
    MinimalBalance,
    lockedPhase,
    NoKeys
}

[System.Serializable]
public class TutorialEventBlock
{
    public TutorialEvent ev;
    public TutorialBlockAsset block;
}

public class TutorialManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] TutorialUI ui;

    [Header("Start Sequence (Tutorial only)")]
    [SerializeField] List<ScheduledBlock> startSequence = new();

    [Header("Event Blocks")]
    [SerializeField] List<TutorialEventBlock> eventBlocks = new();

    [Header("Behavior")]
    [SerializeField] bool pauseGameWhileDialogue = true;
    [SerializeField] bool allowQueue = false;

    [Header("Input Lock (IMPORTANT)")]
    [SerializeField] PlayerInput playerInput;                 // el PlayerInput del jugador
    [SerializeField] string gameplayMapName = "Player";        // tu mapa real
    [SerializeField] string balanceMapName = "Balance";        // tu mapa balance
    [SerializeField] string tutorialMapName = "UI";            // opcional (si existe)
    [SerializeField] bool useTutorialMap = false;
    [SerializeField] ActionMapSwitcher mapSwitcher;


    public bool IsDialogueActive => _running; // o ui.IsOpen si prefieres
    public bool IsBlockingGameplayNow => _running && _current != null && _current.pauseWhileShowing;

    string _prevActionMapName;
    bool _inputLockedByTutorial;

    readonly HashSet<string> _playedBlocks = new();
    readonly Dictionary<TutorialEvent, TutorialBlockAsset> _eventMap = new();

    readonly Queue<TutorialStepAsset> _queue = new();

    TutorialStepAsset _current;
    bool _running;
    Coroutine _autoCloseRoutine;

    void Awake()
    {
        // build map
        _eventMap.Clear();
        for (int i = 0; i < eventBlocks.Count; i++)
        {
            var eb = eventBlocks[i];
            if (eb == null || eb.block == null) continue;
            _eventMap[eb.ev] = eb.block;
        }
    }

    // ---------- Helper: Lock/Unlock ----------
    void LockGameplayInput()
    {
        if (_inputLockedByTutorial) return;
        _inputLockedByTutorial = true;

        mapSwitcher?.PushUI();
    }

    void UnlockGameplayInput()
    {
        if (!_inputLockedByTutorial) return;
        _inputLockedByTutorial = false;

        mapSwitcher?.Pop();
    }
    bool IsPausingStep(TutorialStepAsset step)
    {
        return step != null && step.pauseWhileShowing;
    }


    void Start()
    {
        if (startSequence != null && startSequence.Count > 0)
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

            if (!allowQueue)
                while (_running) yield return null;

            PlayBlockOnce(s.block);
        }
    }

    // ---------- Public API ----------
    public void PlayEventOnce(TutorialEvent ev)
    {
        if (_eventMap.TryGetValue(ev, out var block) && block != null)
            PlayBlockOnce(block);
    }

    public void PlayBlockOnce(TutorialBlockAsset block)
    {
        if (block == null) return;
        if (string.IsNullOrEmpty(block.blockId)) return;

        if (_playedBlocks.Contains(block.blockId)) return;
        if (_running && !allowQueue) return;

        _playedBlocks.Add(block.blockId);

        for (int i = 0; i < block.steps.Count; i++)
            if (block.steps[i] != null) _queue.Enqueue(block.steps[i]);

        if (!_running)
            NextStep();
    }

    // ---------- Input (dialogue) ----------
    void Update()
    {
        if (!_running || ui == null || !ui.IsOpen) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            ui.SkipTyping();

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

    // ---------- en ShowStepWithDelay ----------
    IEnumerator ShowStepWithDelay(TutorialStepAsset step)
    {
        float d = Mathf.Max(0f, step.delayBeforeShow);
        float t = 0f;

        while (t < d)
        {
            t += step.delayUsesUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        // si este paso va a pausar, bloquea input ANTES de mostrar
        if (IsPausingStep(step))
        {
            LockGameplayInput();
            if (pauseGameWhileDialogue) Time.timeScale = 0f;
        }
        else
        {
            // si este paso NO pausa, asegúrate de no dejarlo bloqueado por error
            UnlockGameplayInput();
            if (pauseGameWhileDialogue) Time.timeScale = 1f;
        }

        ui.Show(step.portrait, step.speakerName, step.text, step.hint);

        // AutoClose...
        if (step.autoCloseByTime && step.autoCloseSeconds > 0f)
            _autoCloseRoutine = StartCoroutine(AutoCloseAfter(step.autoCloseSeconds));
    }

    IEnumerator AutoCloseAfter(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        TryContinue(force: true);
    }

    void TryContinue(bool force = false)
    {
        if (_current == null) return;

        if (_current.advanceMode == TutorialAdvanceMode.PressContinue || force)
        {
            ui.Hide();

            // reanuda tiempo si el paso pausaba
            if (pauseGameWhileDialogue) Time.timeScale = 1f;

            // desbloquea input
            UnlockGameplayInput();

            NextStep();
        }
    }
    void EndDialogue()
    {
        _running = false;
        _current = null;
        ui.Hide();
        if (pauseGameWhileDialogue) Time.timeScale = 1f;

        // desbloquea al terminar TODO
        UnlockGameplayInput();
    }


}