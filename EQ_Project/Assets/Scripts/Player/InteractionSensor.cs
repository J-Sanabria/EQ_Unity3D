using System.Collections.Generic;
using UnityEngine;
using TMPro;
using StarterAssets;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class InteractionSensor : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private LayerMask interactableMask;
    [Range(0f, 1f)]
    [SerializeField] private float angleBiasDot = 0.35f;

    [Header("Referencias")]
    [SerializeField] private Transform cameraPivot; // referencia de cámara (isométrica)
    [SerializeField] private TMP_Text promptText;
    [SerializeField] TutorialManager tutorial;
    bool _firedSeen = false;
    public bool _isTutorial = true;

    [Header("HUD Prompt")]
    [SerializeField] private GameObject panelPrompt;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private float fadeSpeed = 8f;

    private readonly List<IInteractable> _nearby = new();
    private IInteractable _current;

    private StarterAssetsInputs _inputs;

    // one-shot interact
    private float _targetAlpha;

    void Awake()
    {
        _firedSeen = false;
        _inputs = GetComponentInParent<StarterAssetsInputs>();
        if (_inputs == null)
            Debug.LogWarning("InteractionSensor: no encontró StarterAssetsInputs en el padre.");
    }

    void OnValidate()
    {
        var sc = GetComponent<SphereCollider>();
        sc.isTrigger = true;
        if (sc.radius < 0.1f) sc.radius = 2.0f;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    void OnEnable()
    {
        SetCurrent(null);
        SetPromptVisible(false, immediate: true);
    }

    void OnDisable()
    {
        SetCurrent(null);
        _nearby.Clear();
        SetPromptVisible(false, immediate: true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & interactableMask.value) == 0) return;

        if (other.TryGetComponent<IInteractable>(out var ia))
        {
            if (!_nearby.Contains(ia))
                _nearby.Add(ia);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IInteractable>(out var ia)) return;

        if (_current == ia) SetCurrent(null);
        _nearby.Remove(ia);
    }

    void Update()
    {
        // Seleccionar mejor candidato
        var best = FindBestCandidate();
        if (best != _current) SetCurrent(best);

        bool show = _current != null;
        if (show && promptText != null)
            promptText.text = _current.Prompt;

        SetPromptVisible(show, immediate: false);

        // Interact one-shot robusto
        if (_inputs == null) return;

        if (_inputs.interact)
        {
            // Consume el input para evitar que se quede "pegado"
            _inputs.interact = false;

            if (_current != null)
                _current.Interact(transform.root);
        }
    }
    public void ResetInteractLatch()
    {
        if (_inputs != null) _inputs.interact = false;
    }
    IInteractable FindBestCandidate()
    {
        IInteractable best = null;
        float bestScore = float.NegativeInfinity;

        Vector3 eye = cameraPivot ? cameraPivot.position : transform.position;
        Vector3 fwd = cameraPivot ? cameraPivot.forward : transform.forward;

        for (int i = _nearby.Count - 1; i >= 0; i--)
        {
            var ia = _nearby[i];
            if (ia == null) { _nearby.RemoveAt(i); continue; }

            var tr = (ia as Component).transform;
            Vector3 to = tr.position - eye;

            float dist = Mathf.Max(0.0001f, to.magnitude);
            float dot = Vector3.Dot(fwd, to.normalized);

            if (dot < angleBiasDot) continue;

            // ponderación simple: mirar más + cerca mejor
            float score = dot * 2f + (1f / dist);
            if (score > bestScore)
            {
                bestScore = score;
                best = ia;
            }
        }

        return best;
    }

    void SetPromptVisible(bool visible, bool immediate)
    {
        if (panelGroup != null)
        {
            if (panelPrompt && !panelPrompt.activeSelf) panelPrompt.SetActive(true);

            _targetAlpha = visible ? 1f : 0f;
            float next = immediate
                ? _targetAlpha
                : Mathf.MoveTowards(panelGroup.alpha, _targetAlpha, fadeSpeed * Time.deltaTime);

            panelGroup.alpha = next;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;

            if (!visible && panelPrompt && panelGroup.alpha <= 0.001f)
                panelPrompt.SetActive(false);
        }
        else
        {
            if (panelPrompt && panelPrompt.activeSelf != visible)
                panelPrompt.SetActive(visible);
        }
    }

    void SetCurrent(IInteractable next)
    {
        if (!_firedSeen && _isTutorial == true){
            tutorial?.PlayEventOnce(TutorialEvent.FirstInteractableSeen);
        }
        if (_current != null) _current.SetFocused(false);
        _current = next;
        if (_current != null) _current.SetFocused(true);

    }
}