using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;   // PlayerInput
using CB.Balance;
using CB.Core;

public class BalanceResultPanel : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] TMP_Text txtTitulo;
    [SerializeField] TMP_Text txtDetalle;
    [SerializeField] TMP_Text txtScore;
    [SerializeField] Button btnContinuar;
    [SerializeField] Button btnReintentar;

    [Header("Opcional")]
    [SerializeField] GameModeController gameMode;
    [SerializeField] BalanceSessionController session;
    [SerializeField] PlayerInput playerInput;       // el PlayerInput del jugador
    [SerializeField] string uiMapName = "UI";       // nombre EXACTO del mapa UI

    string _prevMap;

    void Reset()
    {
        if (gameMode == null) gameMode = FindObjectOfType<GameModeController>();
        if (session == null) session = FindObjectOfType<BalanceSessionController>();
        if (playerInput == null) playerInput = FindObjectOfType<PlayerInput>();
    }

    void Awake()
    {
        if (gameMode == null) gameMode = FindObjectOfType<GameModeController>();
        if (session == null) session = FindObjectOfType<BalanceSessionController>();
        if (playerInput == null) playerInput = FindObjectOfType<PlayerInput>();

        if (btnContinuar) btnContinuar.onClick.AddListener(OnContinuar);
        if (btnReintentar) btnReintentar.onClick.AddListener(OnReintentar);

        if (session != null) session.OnChallengeCompleted += OnCompleted;

        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (session != null) session.OnChallengeCompleted -= OnCompleted;
    }

    void OnCompleted(BalanceResult r)
    {
        // guarda puntaje (opcional)
        try { if (UserDB.Instance != null) UserDB.Instance.AddScore(r.score); } catch { }

        if (txtTitulo) txtTitulo.text = "¡Ecuación balanceada!";
        if (txtDetalle) txtDetalle.text = "Tiempo: " + Mathf.RoundToInt(r.timeSeconds) + " s   Errores: " + r.errors;
        if (txtScore) txtScore.text = "Puntaje: " + r.score;

        // switch a UI y seleccionar primer botón
        if (playerInput != null && !string.IsNullOrEmpty(uiMapName))
        {
            _prevMap = playerInput.currentActionMap != null ? playerInput.currentActionMap.name : "";
            playerInput.SwitchCurrentActionMap(uiMapName);
        }

        gameObject.SetActive(true);

        // fija el foco en el botón Continuar
        if (btnContinuar != null)
            EventSystem.current?.SetSelectedGameObject(btnContinuar.gameObject);
    }

    void RestorePrevMap()
    {
        if (playerInput != null && !string.IsNullOrEmpty(_prevMap))
            playerInput.SwitchCurrentActionMap(_prevMap);
        _prevMap = null;
    }

    void OnContinuar()
    {
        gameObject.SetActive(false);
        RestorePrevMap();
        if (gameMode != null) gameMode.ExitBalance();
    }

    void OnReintentar()
    {
        gameObject.SetActive(false);
        RestorePrevMap();
        if (session != null) session.RestartChallenge();
    }
}
