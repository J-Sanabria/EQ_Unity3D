using UnityEngine;
using CB.Balance;

public class BalanceTutorialEventsBridge : MonoBehaviour
{
    [SerializeField] TutorialManager tutorial;
    [SerializeField] BalanceSessionController session;

    [Header("Anti-spam")]
    [SerializeField] float cooldown = 0.75f;

    float _tNoKeys;
    float _tLocked;
    float _tWrong;

    bool _didFirstAdjust;
    string _reactionIdAtFirstAdjust;
    void Reset()
    {
        if (tutorial == null) tutorial = FindFirstObjectByType<TutorialManager>();
        if (session == null) session = FindFirstObjectByType<BalanceSessionController>();
    }

    void OnEnable()
    {
        if (session != null)
        {
            session.OnAdjustBlocked += OnBlocked;
            session.OnAdjustedApplied += OnAdjusted;
        }
    }

    void OnDisable()
    {
        if (session != null)
        {
            session.OnAdjustBlocked -= OnBlocked;
            session.OnAdjustedApplied -= OnAdjusted;
        }
    }
    void OnBlocked(AdjustBlockReason r)
    {
        if (tutorial == null) return;

        float now = Time.unscaledTime;

        switch (r)
        {
            case AdjustBlockReason.NoKeys:
                if (now - _tNoKeys < cooldown) return;
                _tNoKeys = now;
                tutorial.PlayEventOnce(TutorialEvent.NoKeys);
                break;

            case AdjustBlockReason.PhaseLocked:
                if (now - _tLocked < cooldown) return;
                _tLocked = now;
                tutorial.PlayEventOnce(TutorialEvent.lockedPhase);
                break;

            case AdjustBlockReason.WrongPhaseOrder:
                if (now - _tWrong < cooldown) return;
                _tWrong = now;
                tutorial.PlayEventOnce(TutorialEvent.lockedPhase);
                // o crea TutorialEvent.WrongOrder si quieres diferenciar
                break;
        }
    }
    void OnAdjusted(int side, int index, int before, int after)
    {
        if (tutorial == null) return;

        // Si cambia la reacción (pasaste a otra ecuación), resetea el “first”
        string rid = session != null && session.Station != null && session.Station.reaction != null
            ? session.Station.reaction.reactionId
            : "";

        if (_reactionIdAtFirstAdjust != rid)
        {
            _reactionIdAtFirstAdjust = rid;
            _didFirstAdjust = false;
        }

        if (_didFirstAdjust) return;

        _didFirstAdjust = true;

        // Aquí disparas tu bloque que explica “qué hiciste exactamente”
        // Usa el evento que quieras (por ejemplo FirstInteract o uno nuevo FirstAdjust)
        tutorial.PlayEventOnce(TutorialEvent.FirstInteract);
    }
}