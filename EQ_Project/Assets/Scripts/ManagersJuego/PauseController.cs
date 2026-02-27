using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] CB.Core.GameModeController gameMode;

    [Header("Input")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] string pauseActionName = "Pause"; // debe existir en ambos maps o en un map global

    bool _paused;

    void OnEnable()
    {
        if (playerInput != null)
            playerInput.onActionTriggered += OnActionTriggered;
    }

    void OnDisable()
    {
        if (playerInput != null)
            playerInput.onActionTriggered -= OnActionTriggered;
    }

    void OnActionTriggered(InputAction.CallbackContext ctx)
    {
        Debug.Log("Si entra a puase");
        if (ctx.action == null) return;
        if (!ctx.performed) return;
        if (ctx.action.name != pauseActionName) return;

        // Solo permitir si el mapa activo es Player
        if (playerInput == null || playerInput.currentActionMap == null) return;
        if (playerInput.currentActionMap.name != "Player") return;

        TogglePause();
    }

    public void TogglePause()
    {
        if (_paused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (_paused) return;
        _paused = true;

        Time.timeScale = 0f;
        if (pausePanel) pausePanel.SetActive(true);

        // bloquear gameplay
        if (gameMode != null)
        {
            // quedarte en el mismo State, pero congelar jugador:
            // lo más simple: forzar exploración y dejar MovementEnabled=false
            gameMode.EnterExploration();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (!_paused) return;
        _paused = false;

        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);

        // volver a permitir gameplay según el estado actual:
        // Si estabas en balanceo antes, necesitarías recordar ese estado.
        // Para prototipo: vuelve a exploración.
        if (gameMode != null)
            gameMode.EnterExploration();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }
}