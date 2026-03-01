using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject settingsPanel;

    [Header("Refs")]
    [SerializeField] CB.Core.GameModeController gameMode;
    [SerializeField] LevelController levelController;

    [Header("Input")]
    [SerializeField] InputActionReference pauseAction;   // Player/Pause (ESC)
    [SerializeField] string allowedActionMap = "Player";  // solo exploración

    bool _paused;
    bool _wasCursorVisible;
    CursorLockMode _wasLockMode;

    void Awake()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void OnEnable()
    {
        if (pauseAction?.action != null)
        {
            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (pauseAction?.action != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    void OnPausePerformed(InputAction.CallbackContext _)
    {
        // Solo permitir en exploración (tu Balance usa ESC para Exit)
        if (gameMode == null) return;
        if (gameMode.State != CB.Core.GameState.Exploration) return;

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

        // Guardar cursor
        _wasCursorVisible = Cursor.visible;
        _wasLockMode = Cursor.lockState;

        Time.timeScale = 0f;

        // Bloquea jugador (manteniendo exploración)
        if (gameMode != null)
            gameMode.EnterExploration(); // garantiza que no quede en balance

        // UI
        if (pausePanel) pausePanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (!_paused) return;
        _paused = false;

        Time.timeScale = 1f;

        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        // Restaurar cursor como lo tengas en gameplay
        Cursor.visible = _wasCursorVisible;
        Cursor.lockState = _wasLockMode;
    }

    // ---------- UI Buttons ----------

    public void UI_Continue()
    {
        Resume();
    }

    public void UI_OpenSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(true);
        if (pausePanel) pausePanel.SetActive(false);
    }

    public void UI_BackFromSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    public void UI_RestartReaction()
    {
        // Reanuda tiempo antes de reiniciar flujo
        Time.timeScale = 1f;
        _paused = false;

        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        if (levelController != null)
            levelController.RestartCurrentReaction();
    }

    public void UI_ExitToMainMenu()
    {
        Time.timeScale = 1f;
        _paused = false;

        SceneManager.LoadScene("MenuInicio");
    }
}