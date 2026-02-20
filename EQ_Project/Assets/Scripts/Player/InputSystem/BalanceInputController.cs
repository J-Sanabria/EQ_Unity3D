using UnityEngine;
using UnityEngine.InputSystem;
using CB.Balance;

public class BalanceInputController : MonoBehaviour
{

    [Header("Refs")]
    [SerializeField] BalanceSelectionController selection;
    [SerializeField] BalanceSessionController session;

    [Header("Actions (mapa Balance)")]
    [SerializeField] InputActionReference moveLeft;
    [SerializeField] InputActionReference moveRight;
    [SerializeField] InputActionReference increase;
    [SerializeField] InputActionReference decrease;
    [SerializeField] InputActionReference verify;
    [SerializeField] InputActionReference exit;

    [Header("Audio (opcional)")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip sfxInc, sfxDec, sfxMove;

    // Eventos (orquestación externa)
    public event System.Action VerifyPressed;
    public event System.Action ExitPressed;


    void Reset()
    {
        if (session == null)
            session = GetComponent<BalanceSessionController>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        Bind(moveLeft, OnMoveLeft);
        Bind(moveRight, OnMoveRight);
        Bind(increase, OnIncrease);
        Bind(decrease, OnDecrease);
        Bind(verify, OnVerify);
        Bind(exit, OnExit);
    }

    void OnDisable()
    {
        Unbind(moveLeft, OnMoveLeft);
        Unbind(moveRight, OnMoveRight);
        Unbind(increase, OnIncrease);
        Unbind(decrease, OnDecrease);
        Unbind(verify, OnVerify);
        Unbind(exit, OnExit);
    }

    #region Input binding

    void Bind(InputActionReference a, System.Action<InputAction.CallbackContext> cb)
    {
        if (a?.action == null) return;
        a.action.performed += cb;
    }

    void Unbind(InputActionReference a, System.Action<InputAction.CallbackContext> cb)
    {
        if (a?.action == null) return;
        a.action.performed -= cb;
    }

    #endregion

    #region Callbacks

    void OnMoveLeft(InputAction.CallbackContext _)
    {
        selection.MoveLeft();
        Play(sfxMove);
    }

    void OnMoveRight(InputAction.CallbackContext _)
    {
        selection.MoveRight();
        Play(sfxMove);
    }
    void OnIncrease(InputAction.CallbackContext _)
    {
        session.Adjust(selection.SelectedSide, selection.SelectedIndex, +1);
        Play(sfxInc);
    }

    void OnDecrease(InputAction.CallbackContext _)
    {
        session.Adjust(selection.SelectedSide, selection.SelectedIndex, -1);
        Play(sfxDec);
    }


    void OnVerify(InputAction.CallbackContext _)
    {
        Debug.Log("[Balance] Verify PRESSED");
        VerifyPressed?.Invoke();
    }


    void OnExit(InputAction.CallbackContext _)
    {
        Debug.Log("[Balance] Exit PRESSED");
        ExitPressed?.Invoke();
    }

    #endregion

    void Play(AudioClip clip)
    {
        if (audioSource && clip)
            audioSource.PlayOneShot(clip);
    }
}
