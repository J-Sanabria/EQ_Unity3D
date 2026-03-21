using UnityEngine;
using CB.Balance;

public class BalanceAlertBridge : MonoBehaviour
{
    [SerializeField] private AlertHUD alertHUD;
    [SerializeField] private BalanceSessionController session;

    [Header("Blocked cooldown")]
    [SerializeField] private float blockedCooldown = 0.75f;

    [Header("Verify cooldown")]
    [SerializeField] private float verifyCooldown = 0.9f;

    [Header("Alert Steps")]
    [SerializeField] private AlertStepAsset alertNoKeys;
    [SerializeField] private AlertStepAsset alertPhaseLocked;
    [SerializeField] private AlertStepAsset alertWrongPhaseOrder;
    [SerializeField] private AlertStepAsset alertNotBalanced;
    [SerializeField] private AlertStepAsset alertNotMinimal;
    [SerializeField] private AlertStepAsset alertBalancedMinimal;

    private float _tNoKeys;
    private float _tLocked;
    private float _tWrongOrder;

    private float _tIncorrect;
    private float _tNotMinimal;
    private float _tBalancedMinimal;

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
                if (alertNoKeys != null) alertHUD.ShowAlert(alertNoKeys);
                break;

            case AdjustBlockReason.PhaseLocked:
                if (now - _tLocked < blockedCooldown) return;
                _tLocked = now;
                if (alertPhaseLocked != null) alertHUD.ShowAlert(alertPhaseLocked);
                break;

            case AdjustBlockReason.WrongPhaseOrder:
                if (now - _tWrongOrder < blockedCooldown) return;
                _tWrongOrder = now;
                if (alertWrongPhaseOrder != null) alertHUD.ShowAlert(alertWrongPhaseOrder);
                break;
        }
    }

    private void OnVerifyFeedback(VerifyResult result)
    {
        if (alertHUD == null) return;

        float now = Time.unscaledTime;

        switch (result)
        {
            case VerifyResult.Incorrect:
                if (now - _tIncorrect < verifyCooldown) return;
                _tIncorrect = now;
                if (alertNotBalanced != null) alertHUD.ShowAlert(alertNotBalanced);
                break;

            case VerifyResult.BalancedNotMinimal:
                if (now - _tNotMinimal < verifyCooldown) return;
                _tNotMinimal = now;
                if (alertNotMinimal != null) alertHUD.ShowAlert(alertNotMinimal);
                break;

            case VerifyResult.BalancedMinimal:
                if (now - _tBalancedMinimal < verifyCooldown) return;
                _tBalancedMinimal = now;
                if (alertBalancedMinimal != null) alertHUD.ShowAlert(alertBalancedMinimal);
                break;
        }
    }
}