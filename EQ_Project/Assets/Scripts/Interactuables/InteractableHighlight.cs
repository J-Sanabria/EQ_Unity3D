using UnityEngine;

public class InteractableHighlight : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Emission")]
    [SerializeField] private bool useEmission = true;
    [SerializeField] private string emissionProperty = "_EmissionColor";
    [SerializeField] private Color normalEmission = Color.black;
    [SerializeField] private Color focusedEmission = Color.white * 1.5f;

    [Header("Scale Pulse (optional)")]
    [SerializeField] private bool pulseScale = false;
    [SerializeField] private Transform pulseTarget;
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 focusedScale = new Vector3(1.03f, 1.03f, 1.03f);
    [SerializeField] private float scaleLerpSpeed = 10f;

    private MaterialPropertyBlock _mpb;
    private bool _isFocused;
    private Vector3 _currentTargetScale;

    void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);

        if (pulseTarget == null)
            pulseTarget = transform;

        _mpb = new MaterialPropertyBlock();
        _currentTargetScale = normalScale;

        ApplyImmediate(false);
    }

    void Update()
    {
        if (pulseScale && pulseTarget != null)
        {
            pulseTarget.localScale = Vector3.Lerp(
                pulseTarget.localScale,
                _currentTargetScale,
                scaleLerpSpeed * Time.deltaTime
            );
        }
    }

    public void SetFocused(bool focused)
    {
        if (_isFocused == focused) return;
        _isFocused = focused;

        ApplyEmission(focused);

        if (pulseScale)
            _currentTargetScale = focused ? focusedScale : normalScale;
    }

    public void ApplyImmediate(bool focused)
    {
        _isFocused = focused;
        ApplyEmission(focused);

        if (pulseScale && pulseTarget != null)
        {
            _currentTargetScale = focused ? focusedScale : normalScale;
            pulseTarget.localScale = _currentTargetScale;
        }
    }

    private void ApplyEmission(bool focused)
    {
        if (!useEmission || targetRenderers == null) return;

        Color emission = focused ? focusedEmission : normalEmission;

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            var r = targetRenderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(emissionProperty, emission);
            r.SetPropertyBlock(_mpb);
        }
    }
}