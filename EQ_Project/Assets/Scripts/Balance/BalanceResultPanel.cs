using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using CB.Balance;
using CB.Core;

public class BalanceResultPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text txtTitulo;
    [SerializeField] TMP_Text txtDetalle;
    [SerializeField] TMP_Text txtScore;
    [SerializeField] Button btnContinuar;
    [SerializeField] Button btnReintentar;

    [Header("Opcional")]
    [SerializeField] GameModeController gameMode;
    [SerializeField] BalanceSessionController session;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] string uiMapName = "UI";

    [Header("Apagar mientras está abierto")]
    [SerializeField] GameObject[] hideWhileOpen;

    string prevMap;
    bool[] prevActives;

    // No hagas nada aquí que dependa de estar activo/inactivo
    void Awake()
    {
        if (gameMode == null) gameMode = FindFirstObjectByType<GameModeController>();
        if (session == null) session = FindFirstObjectByType<BalanceSessionController>();
        if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInput>();
        // NO hagas SetActive(false) aquí si el objeto ya está desactivado en el Inspector.
    }

    // Se llama cada vez que el panel se muestra (SetActive(true))
    void OnEnable()
    {
        if (btnContinuar != null)
        {
            btnContinuar.onClick.RemoveListener(OnContinuar);
            btnContinuar.onClick.AddListener(OnContinuar);
        }
        if (btnReintentar != null)
        {
            btnReintentar.onClick.RemoveListener(OnReintentar);
            btnReintentar.onClick.AddListener(OnReintentar);
        }
    }

    void OnDisable()
    {
        // Limpia listeners para no duplicarlos si se vuelve a abrir
        if (btnContinuar != null) btnContinuar.onClick.RemoveListener(OnContinuar);
        if (btnReintentar != null) btnReintentar.onClick.RemoveListener(OnReintentar);
    }

    public void Show(BalanceResult r)
    {
        if (txtTitulo) txtTitulo.text = "¡Ecuación balanceada!";
        if (txtDetalle) txtDetalle.text = "Tiempo: " + Mathf.RoundToInt(r.timeSeconds) + " s   Errores: " + r.errors;
        if (txtScore) txtScore.text = "Puntaje: " + r.score;

        // Cambia al mapa UI (para navegación con teclado/gamepad)
        if (playerInput && !string.IsNullOrEmpty(uiMapName))
        {
            prevMap = playerInput.currentActionMap != null ? playerInput.currentActionMap.name : "";
            playerInput.SwitchCurrentActionMap(uiMapName);
        }

        // Cursor visible para UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Apaga HUDs y recuerda estado
        if (hideWhileOpen != null && hideWhileOpen.Length > 0)
        {
            prevActives = new bool[hideWhileOpen.Length];
            for (int i = 0; i < hideWhileOpen.Length; i++)
            {
                if (hideWhileOpen[i] == null) continue;
                prevActives[i] = hideWhileOpen[i].activeSelf;
                hideWhileOpen[i].SetActive(false);
            }
        }

        // Mostrar panel (si estaba desactivado en el Inspector)
        gameObject.SetActive(true);
    }

    void RestoreEnv()
    {
        if (playerInput && !string.IsNullOrEmpty(prevMap))
            playerInput.SwitchCurrentActionMap(prevMap);
        prevMap = null;

        if (hideWhileOpen != null && prevActives != null)
        {
            for (int i = 0; i < hideWhileOpen.Length; i++)
            {
                if (hideWhileOpen[i] == null) continue;
                hideWhileOpen[i].SetActive(prevActives[i]);
            }
        }
        prevActives = null;
    }

    void OnContinuar()
    {
        gameObject.SetActive(false);
        RestoreEnv();
        if (gameMode) gameMode.ExitBalance();
    }

    void OnReintentar()
    {
        gameObject.SetActive(false);
        RestoreEnv();
        if (session) session.RestartChallenge();
    }
}
