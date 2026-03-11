using UnityEngine;
using CB.Balance;

public class BalanceAudioFeedback : MonoBehaviour
{
    [SerializeField] private BalanceSessionController session;

    [Header("Verify SFX")]
    [SerializeField] private AudioClip correctSfx;
    [SerializeField] private AudioClip incorrectSfx;
    [SerializeField] private AudioClip notMinimalSfx;

    [Header("Volumes")]
    [SerializeField] private float correctVolume = 1f;
    [SerializeField] private float incorrectVolume = 1f;
    [SerializeField] private float notMinimalVolume = 1f;

    private void OnEnable()
    {
        if (session != null)
            session.OnVerifyFeedback += HandleVerifyFeedback;
    }

    private void OnDisable()
    {
        if (session != null)
            session.OnVerifyFeedback -= HandleVerifyFeedback;
    }

    private void HandleVerifyFeedback(VerifyResult result)
    {
        if (AudioManager.Instance == null) return;

        switch (result)
        {
            case VerifyResult.BalancedMinimal:
                AudioManager.Instance.PlaySfx(correctSfx, correctVolume);
                break;

            case VerifyResult.BalancedNotMinimal:
                AudioManager.Instance.PlaySfx(notMinimalSfx, notMinimalVolume);
                break;

            case VerifyResult.Incorrect:
                AudioManager.Instance.PlaySfx(incorrectSfx, incorrectVolume);
                break;
        }
    }
}