using System.Collections.Generic;
using UnityEngine;
using TMPro;
using StarterAssets;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class InteractionSensor : MonoBehaviour
{
    [Header("Deteccion")]
    public LayerMask interactableMask;
    [Range(0f, 1f)] public float angleBiasDot = 0.35f;

    [Header("Referencias")]
    public Transform cameraPivot;   // CinemachineCameraTarget del player
    public TMP_Text promptText;     // Texto que va dentro del panel

    [Header("HUD Prompt")]
    public GameObject panelPrompt;  // GameObject del panel en el Canvas
    public CanvasGroup panelGroup;  // Opcional: si lo asignas, hará fade
    public float fadeSpeed = 8f;    // Velocidad de fade (CanvasGroup)

    private readonly List<IInteractable> _nearby = new List<IInteractable>();
    private IInteractable _current;
    private StarterAssetsInputs _inputs;
    private float _targetAlpha = 0f;

    void Awake()
    {
        _inputs = GetComponentInParent<StarterAssetsInputs>();
        if (_inputs == null) Debug.LogWarning("InteractionSensor: no encontro StarterAssetsInputs en el padre.");
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
        // Panel activo pero transparente si hay CanvasGroup; si no hay, lo ocultamos
        if (panelGroup != null)
        {
            if (panelPrompt && !panelPrompt.activeSelf) panelPrompt.SetActive(true);
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }
        else
        {
            if (panelPrompt) panelPrompt.SetActive(false);
        }
        // No apagues el promptText por separado; vive dentro del panel
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & interactableMask.value) == 0) return;
        var ia = other.GetComponent<IInteractable>();
        if (ia != null && !_nearby.Contains(ia)) _nearby.Add(ia);
    }

    void OnTriggerExit(Collider other)
    {
        var ia = other.GetComponent<IInteractable>();
        if (ia != null)
        {
            if (_current == ia) SetCurrent(null);
            _nearby.Remove(ia);
        }
    }

    void Update()
    {
        // Elegir mejor candidato
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

            float score = dot * 2f + 1f / dist;
            if (score > bestScore) { bestScore = score; best = ia; }
        }

        if (best != _current) SetCurrent(best);

        // Mostrar/ocultar panel y texto
        bool show = _current != null;

        if (show && promptText != null)
            promptText.text = _current.Prompt;

        if (panelGroup != null)
        {
            // Fade con CanvasGroup
            _targetAlpha = show ? 1f : 0f;

            if (panelPrompt && !panelPrompt.activeSelf) panelPrompt.SetActive(true);
            panelGroup.alpha = Mathf.MoveTowards(panelGroup.alpha, _targetAlpha, fadeSpeed * Time.deltaTime);
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;

            // Si quieres ocultar por completo el GO cuando alpha llega a 0 (opcional):
            if (panelGroup.alpha <= 0.001f && !show && panelPrompt && panelPrompt.activeSelf)
                panelPrompt.SetActive(false);
        }
        else
        {
            // Sin CanvasGroup: activacion directa del panel
            if (panelPrompt && panelPrompt.activeSelf != show)
                panelPrompt.SetActive(show);
        }

        // Interact
        bool pressed = _inputs != null ? _inputs.interact : Input.GetKeyDown(KeyCode.E);
        if (_current != null && pressed)
        {
            if (_inputs != null) _inputs.interact = false;
            _current.Interact(transform.root);
        }
    }

    void SetCurrent(IInteractable next)
    {
        if (_current != null) _current.SetFocused(false);
        _current = next;
        if (_current != null) _current.SetFocused(true);
    }
}
