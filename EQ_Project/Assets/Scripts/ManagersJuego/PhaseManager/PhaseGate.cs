using System.Collections;
using UnityEngine;

public class PhaseGate : MonoBehaviour
{
    public enum GateMode
    {
        Locked,
        Open,
        NotPresent // no bloquea, se puede ocultar o dejar abierta
    }

    [Header("Setup")]
    [SerializeField] PhaseKey phase;
    [SerializeField] Transform door;          // el mesh o root que se mueve
    [SerializeField] float openHeight = 3f;
    [SerializeField] float animSeconds = 0.35f;
    [SerializeField] bool disableColliderWhenOpen = true;
    [SerializeField] Collider gateCollider;

    Vector3 _closedPos;
    Vector3 _openPos;
    Coroutine _anim;
    GateMode _mode;

    public PhaseKey Phase => phase;

    void Awake()
    {
        if (door == null) door = transform;
        if (gateCollider == null) gateCollider = GetComponent<Collider>();

        _closedPos = door.localPosition;
        _openPos = _closedPos + Vector3.up * openHeight;
    }

    public void SetMode(GateMode mode, bool instant = false)
    {
        _mode = mode;

        if (mode == GateMode.NotPresent)
        {
            // opción A: quitar puerta
            // gameObject.SetActive(false); return;

            // opción B: dejarla abierta siempre
            SetOpen(true, instant);
            return;
        }

        if (mode == GateMode.Open) SetOpen(true, instant);
        else SetOpen(false, instant);
    }

    void SetOpen(bool open, bool instant)
    {
        if (_anim != null) StopCoroutine(_anim);

        if (instant)
        {
            door.localPosition = open ? _openPos : _closedPos;
            ApplyCollider(open);
            return;
        }

        _anim = StartCoroutine(AnimTo(open ? _openPos : _closedPos, open));
    }

    IEnumerator AnimTo(Vector3 target, bool open)
    {
        Vector3 start = door.localPosition;
        float t = 0f;

        while (t < animSeconds)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / animSeconds);
            float e = 1f - Mathf.Pow(1f - k, 3f); // easeOut
            door.localPosition = Vector3.Lerp(start, target, e);
            yield return null;
        }

        door.localPosition = target;
        ApplyCollider(open);
        _anim = null;
    }

    void ApplyCollider(bool open)
    {
        if (gateCollider == null) return;
        if (disableColliderWhenOpen)
            gateCollider.enabled = !open;
    }
}