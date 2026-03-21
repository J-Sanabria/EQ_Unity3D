using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject panelInicio;
    [SerializeField] GameObject panelConfiguracion;
    [SerializeField] GameObject panelEscogerRol;
    [SerializeField] GameObject panelSalir;
    [SerializeField] GameObject panelEstudiante; // panel de estudiante
    [SerializeField] GameObject panelProfesor;   // panel de profesor
    [SerializeField] GameObject panelCrearUsuario;
    [SerializeField] private SettingsController settingsController;

    [Header("UI Keyboard Navigation")]
    [SerializeField] private UIMenuSelectionDriver uiSelectionDriver;

    [SerializeField] private GameObject firstInicio;
    [SerializeField] private GameObject firstConfiguracion;
    [SerializeField] private GameObject firstEscogerRol;
    [SerializeField] private GameObject firstSalir;
    [SerializeField] private GameObject firstEstudiante;
    [SerializeField] private GameObject firstProfesor;
    [SerializeField] private GameObject firstCrearUsuario;
    [SerializeField] private GameObject firstConfirmUsuario;

    [SerializeField] LevelConfig tutorialLevelConfig; // asigna en inspector
    [SerializeField] string tutorialSceneName = "Tutorial";

    [Header("Confirmación de usuario")]
    [SerializeField] GameObject panelConfirmUsuario;
    [SerializeField] TMP_Text txtUsuario;

    const string PREF_ROLE = "user_role";


    // --- Navegación ---
    readonly Stack<GameObject> navStack = new Stack<GameObject>();
    GameObject current;

    void Awake()
    {
        // Estado inicial
        ForceMenuCursor();

        ShowOnly(panelInicio, clearStack: true);
    }

    // =========================================================
    // Utilidades de navegación
    // =========================================================
    void ShowOnly(GameObject target, bool clearStack = false)
    {
        if (panelCrearUsuario) panelCrearUsuario.SetActive(false);
        if (panelInicio) panelInicio.SetActive(false);
        if (panelConfiguracion) panelConfiguracion.SetActive(false);
        if (panelEscogerRol) panelEscogerRol.SetActive(false);
        if (panelSalir) panelSalir.SetActive(false);
        if (panelEstudiante) panelEstudiante.SetActive(false);
        if (panelProfesor) panelProfesor.SetActive(false);
        if (panelConfirmUsuario) panelConfirmUsuario.SetActive(false);

        if (clearStack) navStack.Clear();

        current = target;
        if (current) current.SetActive(true);

        uiSelectionDriver?.SetFirstSelected(GetFirstSelectedForPanel(current), clearCurrentSelection: true);
    }

    // Empuja el panel actual y muestra el nuevo
    void GoTo(GameObject target)
    {
        if (current != null) navStack.Push(current);
        ShowOnly(target);
    }

    // Volver al anterior (si no hay, vuelve a Inicio)
    public void OnClick_Volver()
    {
        if (navStack.Count > 0)
        {
            var prev = navStack.Pop();
            ShowOnly(prev);
        }
        else
        {
            ShowOnly(panelInicio, clearStack: true);
        }
    }

    // =========================================================
    // Botones principales
    // =========================================================
    public void OnClick_Iniciar() => GoTo(panelEscogerRol);
    public void OnClick_Configuracion()
    {
        GoTo(panelConfiguracion);
        settingsController?.RefreshUI();
    }
    public void OnClick_Salir() => GoTo(panelSalir);
    public void OnClick_VolverMenuInicio() => ShowOnly(panelInicio, clearStack: true);

    // =========================================================
    // Escoger rol
    // =========================================================
    public void OnClick_Rol_Estudiante()
    {
        PlayerPrefs.SetString(PREF_ROLE, "Estudiante");
        PlayerPrefs.Save();

        if (panelEstudiante != null) GoTo(panelEstudiante);
        else Debug.Log("Abrir flujo de Estudiante (cargar escena o activar panel).");
    }

    public void OnClick_Rol_Profesor()
    {
        PlayerPrefs.SetString(PREF_ROLE, "Profesor");
        PlayerPrefs.Save();

        if (panelProfesor != null) GoTo(panelProfesor);
        else Debug.Log("Abrir flujo de Profesor (cargar escena o activar panel).");
    }
        public void OnClick_IrACrearUsuario()
    {
        if (panelCrearUsuario != null) GoTo(panelCrearUsuario);
    }

    // =========================================================
    // Confirmar salida
    // =========================================================
    public void OnClick_Salir_Si()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClick_Salir_No()
    {
        OnClick_Volver(); // regresa al panel anterior (Inicio)
    }

    // =========================================================
    // Tecla ESC = volver
    // =========================================================
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnClick_Volver();
    }
    public void OnStudentPicked()
    {
        if (UserDB.Instance == null)
        {
            Debug.LogWarning("UserDB no está listo.");
            return;
        }

        string user = UserDB.Instance.GetCurrentUser();

        if (string.IsNullOrEmpty(user))
        {
            Debug.LogWarning("No hay usuario seleccionado/creado.");
            // Opcional: volver al panel Estudiante para que elijan/creen
            OnClick_Volver();
            return;
        }

        // Siempre mostrar confirmación con el nombre elegido
        if (txtUsuario) txtUsuario.text = ("Quieres comenzar el juego con el usuario ") + user;
        GoTo(panelConfirmUsuario);
    }

    // Botón: Continuar
    public void OnClick_Confirm_Continuar()
    {
        StartGame();
    }

    void StartGame()
    {
        if (UserDB.Instance == null)
        {
            Debug.LogWarning("[Menu] UserDB no está listo.");
            return;
        }

        string user = UserDB.Instance.GetCurrentUser();
        if (string.IsNullOrEmpty(user))
        {
            Debug.LogWarning("[Menu] No hay usuario actual.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[Menu] No existe GameManager en escena.");
            return;
        }

        GameManager.Instance.SetCurrentUser(user);
        GameManager.Instance.StartNewGame(tutorialSceneName, tutorialLevelConfig);
    }

    private GameObject GetFirstSelectedForPanel(GameObject panel)
    {
        if (panel == panelInicio) return firstInicio;
        if (panel == panelConfiguracion) return firstConfiguracion;
        if (panel == panelEscogerRol) return firstEscogerRol;
        if (panel == panelSalir) return firstSalir;
        if (panel == panelEstudiante) return firstEstudiante;
        if (panel == panelProfesor) return firstProfesor;
        if (panel == panelCrearUsuario) return firstCrearUsuario;
        if (panel == panelConfirmUsuario) return firstConfirmUsuario;
        return null;
    }
    private void ForceMenuCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
