using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject root;
    [SerializeField] Image portrait;
    [SerializeField] TMP_Text speakerText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] TMP_Text hintText;

    [SerializeField] CanvasGroup group;
    [SerializeField] RectTransform panelRoot;
    [SerializeField] float animDuration = 0.12f;
    [SerializeField] Vector3 hiddenScale = new Vector3(0.92f, 0.92f, 1f);

    Coroutine _anim;

    [Header("Typewriter")]
    [SerializeField] float charsPerSecond = 40f;

    Coroutine _typing;
    string _fullText;
    bool _isTyping;

    public bool IsOpen => root != null && root.activeSelf;
    public bool IsTyping => _isTyping;

    void Awake()
    {
        if (root) root.SetActive(false);
    }

    public void Show(Sprite portraitSprite, string speaker, string text, string hint)
    {
        if (root) root.SetActive(true);
        if (portrait) portrait.sprite = portraitSprite;
        if (speakerText) speakerText.text = speaker ?? "";
        if (hintText) hintText.text = hint ?? "";

        PlayOpenAnim();
        StartTyping(text ?? "");
    }
    public void Hide()
    {
        StopTyping();
        if (root) root.SetActive(false);
    }

    public void SkipTyping()
    {
        if (!_isTyping) return;
        StopTyping();
        if (bodyText) bodyText.text = _fullText;
        _isTyping = false;
    }

    void StartTyping(string text)
    {
        StopTyping();
        _fullText = text;
        _typing = StartCoroutine(TypeRoutine(text));
    }

    void StopTyping()
    {
        if (_typing != null) StopCoroutine(_typing);
        _typing = null;
        _isTyping = false;
    }

    IEnumerator TypeRoutine(string text)
    {
        _isTyping = true;
        if (bodyText) bodyText.text = "";

        float t = 0f;
        int shown = 0;

        while (shown < text.Length)
        {
            t += Time.unscaledDeltaTime * charsPerSecond; // unscaled para cuando pausas
            int next = Mathf.Min(text.Length, Mathf.FloorToInt(t));
            if (next != shown)
            {
                shown = next;
                if (bodyText) bodyText.text = text.Substring(0, shown);
            }
            yield return null;
        }

        _isTyping = false;
    }

    void PlayOpenAnim()
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(OpenAnim());
    }

    IEnumerator OpenAnim()
    {
        if (group) group.alpha = 0f;
        if (panelRoot) panelRoot.localScale = hiddenScale;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / animDuration);
            // easeOut
            float e = 1f - Mathf.Pow(1f - k, 3f);

            if (group) group.alpha = e;
            if (panelRoot) panelRoot.localScale = Vector3.Lerp(hiddenScale, Vector3.one, e);
            yield return null;
        }

        if (group) group.alpha = 1f;
        if (panelRoot) panelRoot.localScale = Vector3.one;
    }
}