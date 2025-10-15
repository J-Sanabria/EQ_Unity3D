using UnityEngine;
using UnityEngine.InputSystem;
using CB.Core;
using CB.Balance;

public class BalanceInputController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameModeController gameMode;
    [SerializeField] BalanceSessionController session;

    [Header("Actions (del mapa 'Balance')")]
    public InputActionReference moveLeft;
    public InputActionReference moveRight;
    public InputActionReference increase;
    public InputActionReference decrease;
    public InputActionReference verify;
    public InputActionReference exitAction;

    [Header("Audio (opcional)")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip sfxInc, sfxDec, sfxMove, sfxOk, sfxError;

    int selSide = 0;   // 0 = izquierda, 1 = derecha
    int selIndex = 0;

    void Reset()
    {
        if (gameMode == null) gameMode = FindObjectOfType<GameModeController>();
        if (session == null) session = GetComponent<BalanceSessionController>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        EnableAction(moveLeft, OnMoveLeft);
        EnableAction(moveRight, OnMoveRight);
        EnableAction(increase, OnIncrease);
        EnableAction(decrease, OnDecrease);
        EnableAction(verify, OnVerify);
        EnableAction(exitAction, OnExit);

        SnapSelection();
        Render();
    }

    void OnDisable()
    {
        DisableAction(moveLeft, OnMoveLeft);
        DisableAction(moveRight, OnMoveRight);
        DisableAction(increase, OnIncrease);
        DisableAction(decrease, OnDecrease);
        DisableAction(verify, OnVerify);
        DisableAction(exitAction, OnExit);
    }

    void EnableAction(InputActionReference aref, System.Action<InputAction.CallbackContext> cb)
    {
        if (aref == null || aref.action == null) return;
        aref.action.performed += cb;
        aref.action.Enable();
    }
    void DisableAction(InputActionReference aref, System.Action<InputAction.CallbackContext> cb)
    {
        if (aref == null || aref.action == null) return;
        aref.action.performed -= cb;
        aref.action.Disable();
    }

    // Callbacks
    void OnMoveLeft(InputAction.CallbackContext ctx)
    {
        if (!InBalance()) return;
        MoveLeft();
    }
    void OnMoveRight(InputAction.CallbackContext ctx)
    {
        if (!InBalance()) return;
        MoveRight();
    }
    void OnIncrease(InputAction.CallbackContext ctx)
    {
        if (!InBalance()) return;
        session.Adjust(selSide, selIndex, +1);
        Play(sfxInc);
        Render();
    }
    void OnDecrease(InputAction.CallbackContext ctx)
    {
        if (!InBalance()) return;
        session.Adjust(selSide, selIndex, -1);
        Play(sfxDec);
        Render();
    }
    void OnVerify(InputAction.CallbackContext ctx)
    {
        if (!InBalance()) return;

        bool ok = session.IsBalancedNow();

        // feedback visual de la balanza
        BalanceVisualController visual = null;
        if (gameMode != null && gameMode.ActiveStation != null)
            visual = gameMode.ActiveStation.balanceVisual;

        if (ok)
        {
            Play(sfxOk);
            if (visual != null) visual.OnVerify(true);

            // NUEVO: cerrar desafío y disparar panel de resultado
            session.CompleteChallenge();
        }
        else
        {
            session.errorCount++;
            Play(sfxError);
            if (visual != null) visual.OnVerify(false);
        }
    }

    void OnExit(InputAction.CallbackContext ctx)
    {
        if (!InBalance()) return;
        gameMode.ExitBalance();
    }

    bool InBalance()
    {
        return gameMode != null && gameMode.State == GameState.Balance && session != null;
    }

    // Navegación
    void MoveLeft()
    {
        int lCount = session.LeftCount;
        int rCount = session.RightCount;

        if (selSide == 1 && rCount > 0)
        {
            if (selIndex > 0) selIndex--;
            else { selSide = 0; selIndex = Mathf.Max(0, lCount - 1); }
        }
        else if (selSide == 0 && lCount > 0)
        {
            selIndex = Mathf.Max(0, selIndex - 1);
        }
        Play(sfxMove);
        Render();
    }

    void MoveRight()
    {
        int lCount = session.LeftCount;
        int rCount = session.RightCount;

        if (selSide == 0 && lCount > 0)
        {
            if (selIndex < lCount - 1) selIndex++;
            else { selSide = 1; selIndex = 0; }
        }
        else if (selSide == 1 && rCount > 0)
        {
            selIndex = Mathf.Min(rCount - 1, selIndex + 1);
        }
        Play(sfxMove);
        Render();
    }

    void SnapSelection()
    {
        int lCount = session.LeftCount;
        int rCount = session.RightCount;

        if (lCount > 0) { selSide = 0; selIndex = Mathf.Clamp(selIndex, 0, lCount - 1); }
        else { selSide = 1; selIndex = Mathf.Clamp(selIndex, 0, Mathf.Max(0, rCount - 1)); }
    }

    void Render()
    {
        session.Render(selSide, selIndex);
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
