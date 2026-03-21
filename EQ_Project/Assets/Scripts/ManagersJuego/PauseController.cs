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

    [Header("UI Navigation")]
    [SerializeField] private UIMenuSelectionDriver uiSelectionDriver;
    [SerializeField] private GameObject firstPauseSelected;
    [SerializeField] private GameObject firstSettingsSelected;

    [Header("Input")]
    [SerializeField] InputActionReference pauseAction;   // Player/Pause (ESC)
    [SerializeField] ActionMapSwitcher mapSwitcher;       // <- IMPORTANTE

    [SerializeField] TutorialManager tutorial;

    bool _paused;
    bool _wasCursorVisible;
    CursorLockMode _wasLockMode;
    float _prevTimeScale = 1f;

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
        }
    }

    void OnDisable()
    {
        if (pauseAction?.action != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
        }
    }

    void OnPausePerformed(InputAction.CallbackContext _)
    {
        if (gameMode == null) return;

        // Si hay UI modal activa (Resultados / Nivel completado / etc), NO PAUSAR
        if (mapSwitcher != null && mapSwitcher.IsUIActive) return;

        // Solo permitir pausa en exploración
        if (gameMode.State != CB.Core.GameState.Exploration) return;

        // Opcional: si el mapa actual no es Player, no pausar
        if (mapSwitcher != null && mapSwitcher.CurrentMapName != "Player") return;

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

        // Guardar cursor + timeScale
        _wasCursorVisible = Cursor.visible;
        _wasLockMode = Cursor.lockState;
        _prevTimeScale = Time.timeScale;

        // Congelar mundo
        Time.timeScale = 0f;

        // Asegura exploración (no balance)
        if (gameMode != null)
            gameMode.EnterExploration();

        // Cambia a UI action map
        mapSwitcher?.PushUI();

        // UI
        if (pausePanel) pausePanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);

        uiSelectionDriver?.SetFirstSelected(firstPauseSelected, clearCurrentSelection: true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (!_paused) return;
        _paused = false;

        // Cierra UI
        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        // Vuelve al action map anterior (Player normalmente)
        mapSwitcher?.Pop();

        // Restaura timeScale/cursor
        Time.timeScale = _prevTimeScale <= 0f ? 1f : _prevTimeScale;
        Cursor.visible = _wasCursorVisible;
        Cursor.lockState = _wasLockMode;
    }

    // ---------- UI Buttons ----------
    public void UI_Continue() => Resume();

    public void UI_OpenSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(true);
        if (pausePanel) pausePanel.SetActive(false);

        uiSelectionDriver?.SetFirstSelected(firstSettingsSelected, clearCurrentSelection: true);
    }

    public void UI_BackFromSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);

        uiSelectionDriver?.SetFirstSelected(firstPauseSelected, clearCurrentSelection: true);
    }

    public void UI_RestartReaction()
    {
        // Cierra pausa correctamente
        Resume();

        // Reinicia
        if (levelController != null)
            levelController.RestartCurrentReaction();
    }

    public void UI_ExitToMainMenu()
    {
        GameManager.Instance?.SaveProgressSoFar();
        Time.timeScale = 1f;
       SceneManager.LoadScene("MenuInicio");
    }


}