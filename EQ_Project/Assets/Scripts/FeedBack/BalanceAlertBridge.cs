using UnityEngine;
using CB.Balance;

public class BalanceAlertBridge : MonoBehaviour
{
    [SerializeField] private AlertHUD alertHUD;
    [SerializeField] private BalanceSessionController session;

    [Header("Blocked cooldown")]
    [SerializeField] private float blockedCooldown = 0.75f;

    private float _tNoKeys;
    private float _tLocked;
    private float _tWrongOrder;

    private void Reset()
    {
        if (alertHUD == null) alertHUD = FindFirstObjectByType<AlertHUD>();
        if (session == null) session = FindFirstObjectByType<BalanceSessionController>();
    }

    private void OnEnable()
    {
        if (session != null)
        {
            session.OnAdjustBlocked += OnAdjustBlocked;
            session.OnVerifyFeedback += OnVerifyFeedback;
        }
    }

    private void OnDisable()
    {
        if (session != null)
        {
            session.OnAdjustBlocked -= OnAdjustBlocked;
            session.OnVerifyFeedback -= OnVerifyFeedback;
        }
    }

    private void OnAdjustBlocked(AdjustBlockReason reason)
    {
        if (alertHUD == null) return;

        float now = Time.unscaledTime;

        switch (reason)
        {
            case AdjustBlockReason.NoKeys:
                if (now - _tNoKeys < blockedCooldown) return;
                _tNoKeys = now;
                alertHUD.ShowAlert("Necesitas recoger una llave para comenzar a balancear.");
                break;

            case AdjustBlockReason.PhaseLocked:
                if (now - _tLocked < blockedCooldown) return;
                _tLocked = now;
                alertHUD.ShowAlert("Esta fase aún está bloqueada.");
                break;

            case AdjustBlockReason.WrongPhaseOrder:
                if (now - _tWrongOrder < blockedCooldown) return;
                _tWrongOrder = now;
                alertHUD.ShowAlert("Debes seguir el orden correcto de balanceo.");
                break;
        }
    }

    private void OnVerifyFeedback(VerifyResult result)
    {
        if (alertHUD == null) return;

        switch (result)
        {
            case VerifyResult.Incorrect:
                alertHUD.ShowAlert("La ecuación aún no está balanceada.");
                break;

            case VerifyResult.BalancedNotMinimal:
                alertHUD.ShowAlert("Está balanceada, pero no está en su mínima expresión.");
                break;

            case VerifyResult.BalancedMinimal:
                alertHUD.ShowAlert("¡Ecuación balanceada correctamente!");
                break;
        }
    }
}