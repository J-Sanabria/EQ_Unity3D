using System.Collections;
using TMPro;
using UnityEngine;

public class AlertHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float defaultDuration = 1.8f;
    [SerializeField] private float fadeInSpeed = 12f;
    [SerializeField] private float fadeOutSpeed = 8f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine _showRoutine;

    private void Awake()
    {
        HideImmediate();
    }

    public void ShowAlert(string message)
    {
        ShowAlert(message, defaultDuration);
    }

    public void ShowAlert(string message, float duration)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        if (_showRoutine != null)
            StopCoroutine(_showRoutine);

        _showRoutine = StartCoroutine(ShowRoutine(message, duration));
    }

    public void HideImmediate()
    {
        if (root != null) root.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    private IEnumerator ShowRoutine(string message, float duration)
    {
        if (messageText != null)
            messageText.text = message;

        if (root != null && !root.activeSelf)
            root.SetActive(true);

        if (canvasGroup != null)
        {
            while (canvasGroup.alpha < 1f)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, fadeInSpeed * dt);
                yield return null;
            }
        }

        float timer = 0f;
        while (timer < duration)
        {
            timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        if (canvasGroup != null)
        {
            while (canvasGroup.alpha > 0f)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeOutSpeed * dt);
                yield return null;
            }
        }

        if (root != null)
            root.SetActive(false);

        _showRoutine = null;
    }
}