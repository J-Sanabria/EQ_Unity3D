using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AlertHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TutorialUI ui;

    [Header("Behavior")]
    [SerializeField] private bool queueAlerts = false;

    [Header("Dismiss Input")]
    [SerializeField] private bool allowManualDismiss = true;
    [SerializeField] private bool dismissWithEnter = true;
    [SerializeField] private bool dismissWithEscape = true;
    [SerializeField] private float manualDismissDelay = 0.12f;

    private readonly Queue<AlertRequest> _queue = new();

    private Coroutine _routine;
    private bool _showing;
    private bool _dismissRequested;
    private bool _canManualDismiss;

    private struct AlertRequest
    {
        public Sprite portrait;
        public string speaker;
        public string text;
        public string hint;
        public float duration;
        public float delay;
        public bool useUnscaledTime;
    }

    private void Awake()
    {
        if (ui != null)
            ui.Hide();
    }

    private void Update()
    {
        if (!_showing || !allowManualDismiss || !_canManualDismiss) return;
        if (Keyboard.current == null) return;

        if (dismissWithEnter && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            _dismissRequested = true;
            return;
        }

        if (dismissWithEscape && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            _dismissRequested = true;
        }
    }

    public void ShowAlert(AlertStepAsset step)
    {
        if (step == null || ui == null) return;

        var req = new AlertRequest
        {
            portrait = step.portrait,
            speaker = step.speakerName,
            text = step.text,
            hint = step.hint,
            duration = step.duration,
            delay = step.delayBeforeShow,
            useUnscaledTime = step.useUnscaledTime
        };

        EnqueueOrShow(req);
    }

    private void EnqueueOrShow(AlertRequest request)
    {
        if (_showing)
        {
            if (queueAlerts)
            {
                _queue.Enqueue(request);
                return;
            }

            if (_routine != null)
                StopCoroutine(_routine);
        }

        _routine = StartCoroutine(ShowRoutine(request));
    }

    private IEnumerator ShowRoutine(AlertRequest request)
    {
        _showing = true;
        _dismissRequested = false;
        _canManualDismiss = false;

        if (request.delay > 0f)
        {
            float t = 0f;
            while (t < request.delay)
            {
                t += request.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }

        ui.Show(request.portrait, request.speaker, request.text, request.hint);
        ui.SkipTyping();

        float dismissTimer = 0f;
        while (dismissTimer < manualDismissDelay)
        {
            dismissTimer += request.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        _canManualDismiss = true;

        float timer = 0f;
        while (timer < request.duration)
        {
            if (_dismissRequested)
                break;

            timer += request.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        ui.Hide();

        _showing = false;
        _dismissRequested = false;
        _canManualDismiss = false;
        _routine = null;

        if (_queue.Count > 0)
        {
            var next = _queue.Dequeue();
            _routine = StartCoroutine(ShowRoutine(next));
        }
    }

    public void HideImmediate()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        _queue.Clear();
        _showing = false;
        _dismissRequested = false;
        _canManualDismiss = false;

        if (ui != null)
            ui.Hide();
    }
}