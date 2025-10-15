using UnityEngine;
using CB.Balance;
using System.Collections;

public class BalanceVisualController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] BalanceSessionController session;   // referencia a la sesión del panel de balance
    [SerializeField] Transform beam;                     // el brazo que rota (si es este mismo, dejar vacío)
    [SerializeField] Renderer beamRenderer;              // opcional, para flash de color

    [Header("Rotación")]
  
    [SerializeField] TiltAxis tiltAxis = TiltAxis.Z;     // eje de inclinación
    [SerializeField] bool invertAxis = false;            // invierte el sentido si hace falta
    [SerializeField] float maxAngle = 18f;               // inclinación máxima en grados
    [SerializeField] float followSpeed = 4f;             // suavizado hacia el target
    [SerializeField] float imbalanceScale = 10f;         // a mayor valor, menos ángulo por desbalance
    public enum TiltAxis { X, Y, Z }

    [Header("Feedback")]
    [SerializeField] Color okColor = new Color(0.6f, 1f, 0.6f);
    [SerializeField] Color errorColor = new Color(1f, 0.4f, 0.4f);
    [SerializeField] float flashTime = 0.15f;
    [SerializeField] float nudgeAngle = 6f;              // pequeño “golpe” al fallar
    [SerializeField] float nudgeReturnSpeed = 10f;

    float _currentAngle;
    float _targetAngle;
    Material _mat;
    Quaternion _baseRot;

    void Reset()
    {
        if (beam == null) beam = transform;
        if (beamRenderer == null) beamRenderer = GetComponentInChildren<Renderer>();
    }

    void Awake()
    {
        if (beam == null) beam = transform;
        _baseRot = beam.localRotation;

        if (beamRenderer != null)
            _mat = beamRenderer.material;
    }

    void Update()
    {
        // Usa la propiedad pública Station del BalanceSessionController
        if (session == null || session.Station == null || session.Station.reaction == null) return;

        var rxn = session.Station.reaction;

        // total de átomos por lado
        int leftAtoms = TotalAtoms(rxn.lhs, session.coefL);
        int rightAtoms = TotalAtoms(rxn.rhs, session.coefR);

        // magnitud de desbalance (suma de |dif| por elemento)
        int mismatch = TotalImbalance(rxn.lhs, rxn.rhs, session.coefL, session.coefR);

        // dirección: lado con MÁS átomos
        float sign = Mathf.Sign(leftAtoms - rightAtoms); // -1, 0, 1
        float t = Mathf.Clamp01(mismatch / Mathf.Max(1f, imbalanceScale));
        _targetAngle = sign * maxAngle * t;

        // suavizado y aplicación
        _currentAngle = Mathf.Lerp(_currentAngle, _targetAngle, Time.deltaTime * followSpeed);
        ApplyRotation(_currentAngle);
    }

    void ApplyRotation(float angle)
    {
        if (invertAxis) angle = -angle;

        Vector3 eul = Vector3.zero;
        switch (tiltAxis)
        {
            case TiltAxis.X: eul.x = angle; break;
            case TiltAxis.Y: eul.y = angle; break;
            case TiltAxis.Z: eul.z = angle; break;
        }

        beam.localRotation = _baseRot * Quaternion.Euler(eul);
    }

    int TotalAtoms(string[] species, int[] coef)
    {
        var side = ReactionValidator.CountSide(species, coef);
        int total = 0;
        foreach (var kv in side) total += kv.Value;
        return total;
    }

    int TotalImbalance(string[] lhs, string[] rhs, int[] coefL, int[] coefR)
    {
        var diff = ReactionValidator.Imbalance(lhs, rhs, coefL, coefR);
        int sum = 0;
        foreach (var kv in diff) sum += Mathf.Abs(kv.Value);
        return sum;
    }

    public void OnVerify(bool ok)
    {
        StopAllCoroutines();
        if (ok)
        {
            StartCoroutine(Flash(okColor));
            _targetAngle = 0f; // vuelve al centro
        }
        else
        {
            StartCoroutine(Flash(errorColor));
            StartCoroutine(Nudge());
        }
    }

    IEnumerator Flash(Color c)
    {
        if (_mat == null) yield break;
        Color original = _mat.color;
        _mat.color = c;
        yield return new WaitForSeconds(flashTime);
        _mat.color = original;
    }

    IEnumerator Nudge()
    {
        float start = _currentAngle;
        float dir = Mathf.Sign(_targetAngle);
        if (dir == 0f) dir = 1f;

        float goal = start + dir * nudgeAngle;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * nudgeReturnSpeed;
            _currentAngle = Mathf.Lerp(start, goal, t);
            ApplyRotation(_currentAngle);
            yield return null;
        }
        // luego Update volverá a perseguir _targetAngle normalmente
    }

    // Permite inyectar la sesión (útil al entrar al modo Balance)
    public void BindSession(BalanceSessionController s)
    {
        session = s;
    }
}
