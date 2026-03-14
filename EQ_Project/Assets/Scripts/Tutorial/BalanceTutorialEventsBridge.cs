using UnityEngine;
using CB.Balance;

public class BalanceTutorialEventsBridge : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorial;
    [SerializeField] private BalanceSessionController session;

    private bool _didFirstAdjust;
    private string _reactionIdAtFirstAdjust;

    private void Reset()
    {
        if (tutorial == null) tutorial = FindFirstObjectByType<TutorialManager>();
        if (session == null) session = FindFirstObjectByType<BalanceSessionController>();
    }

    private void OnEnable()
    {
        if (session != null)
        {
            session.OnAdjustedApplied += OnAdjusted;
            session.OnVerifyFeedback += OnVerifyFeedback;
        }
    }

    private void OnDisable()
    {
        if (session != null)
        {
            session.OnAdjustedApplied -= OnAdjusted;
            session.OnVerifyFeedback -= OnVerifyFeedback;
        }
    }

    private void OnAdjusted(int side, int index, int before, int after)
    {
        if (tutorial == null || session == null) return;

        string rid = session.Station != null && session.Station.reaction != null
            ? session.Station.reaction.reactionId
            : "";

        if (_reactionIdAtFirstAdjust != rid)
        {
            _reactionIdAtFirstAdjust = rid;
            _didFirstAdjust = false;
        }

        if (_didFirstAdjust) return;

        _didFirstAdjust = true;
        tutorial.PlayEventOnce(TutorialEvent.FirstInteract);
    }

    private void OnVerifyFeedback(VerifyResult result)
    {
        if (tutorial == null) return;

        if (result == VerifyResult.BalancedNotMinimal)
            tutorial.PlayEventOnce(TutorialEvent.MinimalBalance);
    }
}