using System.Collections.Generic;
using UnityEngine;
using CB.Balance;
using CB.Core;

public class LevelController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private LevelConfig levelConfig;

    [Header("Refs")]
    [SerializeField] private BalanceStation balanceStation;
    [SerializeField] private GameModeController gameMode;
    [SerializeField] private BalanceResultPanel resultPanel;
    [SerializeField] private EquationHUDBinding equationHUD;
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private PhaseKeyActivator keyActivator;
    [SerializeField] private PlayerKeyRing playerKeyRing;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private PlayerRespawner playerRespawner;
    [SerializeField] PhaseGateController gateController;
    [SerializeField] ActionMapSwitcher mapSwitcher; // asigna en inspector
    [SerializeField] GameObject nextLevelDoor;

    [Header("Next Level")]
    [SerializeField] string easySceneName = "Easy";     // nombre de la escena del nivel fácil
    [SerializeField] LevelConfig easyLevelConfig;       // LevelConfig del nivel fácil
    [SerializeField] bool autoAdvanceToEasy = true;     // por si luego quieres volver a puerta

    private LevelPhase phase = LevelPhase.Exploration;

    private List<ReactionAsset> selectedReactions = new();
    private int currentIndex = -1;
    int[] scoreByReaction; // mismo tamaño que selectedReactions



    // métricas acumuladas
    private float totalTime;
    private int totalErrors;
    private int totalScore;

#if UNITY_EDITOR
    [Header("Editor Only - Auto Start (para probar sin menú)")]
    [SerializeField] bool editorAutoStart = true;
    [SerializeField] string editorUser = "TestUser";
#endif

    void Start()
    {

#if UNITY_EDITOR
        if (!editorAutoStart) return;

        // Si GameManager ya pidió iniciar (venimos del menú), NO hagas nada.
        if (GameManager.Instance != null && GameManager.Instance.WillAutoStartLevel())
            return;

        // Si no hay config asignada en el inspector, no puede arrancar.
        if (levelConfig == null)
        {
            Debug.LogWarning("[LevelController] EditorAutoStart: falta LevelConfig asignado en el inspector.");
            return;
        }

        // Asegura un usuario para que Score/DB no fallen (sin pasar por menú).
        if (UserDB.Instance != null)
        {
            if (string.IsNullOrEmpty(UserDB.Instance.GetCurrentUser()))
                UserDB.Instance.SetCurrentUser(editorUser);
        }

        // Arranca el nivel usando el LevelConfig del inspector
        StartLevel(levelConfig);
#endif
    }

    void Awake()
    {
        if (resultPanel != null)
        {
            resultPanel.OnContinueRequested += Continue;
            resultPanel.OnRetryRequested += RetryCurrentReaction;
        }
    }

    void OnEnable()
    {
        if (balanceStation != null && balanceStation.session != null)
            balanceStation.session.OnSessionCompleted += HandleSessionCompleted;

        if (gameMode != null)
            gameMode.OnStateChanged += HandleGameModeState;
    }

    void EnsureScoreArray()
    {
        if (selectedReactions == null) return;

        scoreByReaction = new int[selectedReactions.Count];
        for (int i = 0; i < scoreByReaction.Length; i++)
            scoreByReaction[i] = -1;
    }
    void OnDisable()
    {
        if (balanceStation != null && balanceStation.session != null)
            balanceStation.session.OnSessionCompleted -= HandleSessionCompleted;

        if (gameMode != null)
            gameMode.OnStateChanged -= HandleGameModeState;
    }

    void HandleGameModeState(GameState state)
    {
        // Si volvimos a exploración sin completar, resetea fase
        if (state == GameState.Exploration && phase == LevelPhase.Balance)
            phase = LevelPhase.Exploration;
    }

    // -------------------------
    // Inicio de nivel
    // -------------------------
    public void StartLevel(LevelConfig config)
    {
        if (config == null || config.reactionPool == null)
        {
            Debug.LogError("[LevelController] LevelConfig inválido");
            return;
        }



        if (balanceStation == null || gameMode == null || resultPanel == null)
        {
            Debug.LogError("[LevelController] Falta referencia (balanceStation/gameMode/resultPanel)");
            return;
        }

        levelConfig = config;

        selectedReactions = PickRandomReactions(config.reactionPool.reactions, config.reactionsPerRun);
        if (selectedReactions.Count == 0)
        {
            Debug.LogError("[LevelController] No hay reacciones seleccionadas");
            return;
        }

        if (equationHUD == null)
            Debug.LogWarning("[LevelController] equationHUD no asignado (solo no se verá HUD).");

        if (phaseManager == null)
        {
            Debug.LogError("[LevelController] Falta PhaseManager asignado");
            return;
        }

        EnsureScoreArray(); 

        currentIndex = 0;
        totalTime = 0f;
        totalErrors = 0;
        totalScore = 0;

        LoadReaction();
    }

    public void RestartCurrentReaction()
    {
        if (balanceStation == null || balanceStation.reaction == null) return;

        // 1) reset llaves
        if (playerKeyRing != null) playerKeyRing.ClearKeys();

        // 2) reset coeficientes a 1 (HARD RESET)
        if (balanceStation.session != null)
        {
            balanceStation.session.BindStation(balanceStation); // asegura Station asignada
            balanceStation.session.ResetCoefsToOnes();
        }

        // 3) refresca HUD ecuación (usa coefL/coefR actuales)
        if (equationHUD != null)
            equationHUD.SetReaction(balanceStation.reaction);

        // 4) reconfig fases/llaves/puertas
        if (phaseManager != null)
            phaseManager.ConfigureForReaction(balanceStation, levelConfig.difficulty);

        if (keyActivator != null)
            keyActivator.SetActiveKeys(phaseManager.GetPresentPhases());

        if (gateController != null)
            gateController.Configure(phaseManager.GetPresentPhases(), levelConfig.difficulty);

        // 5) respawn
        if (playerRespawner != null && spawnPoint != null)
            playerRespawner.RespawnAt(spawnPoint);

        // 6) vuelve a exploración
        phase = LevelPhase.Exploration;
        gameMode.EnterExploration();

        // si ya la habías completado antes, quita ese score acumulado
        if (scoreByReaction != null && currentIndex >= 0 && currentIndex < scoreByReaction.Length)
        {
            int old = scoreByReaction[currentIndex];
            if (old >= 0)
            {
                totalScore -= old;
                scoreByReaction[currentIndex] = -1;
            }
        }

    }

    // -------------------------
    // Selección de reacciones
    // -------------------------
    private static List<ReactionAsset> PickRandomReactions(List<ReactionAsset> source, int count)
    {
        if (source == null || source.Count == 0 || count <= 0)
            return new List<ReactionAsset>();

        if (source.Count <= count)
            return new List<ReactionAsset>(source);

        var copy = new List<ReactionAsset>(source);
        var result = new List<ReactionAsset>(count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, copy.Count);
            result.Add(copy[index]);
            copy.RemoveAt(index);
        }

        return result;
    }

    // -------------------------
    // Flujo de reacción
    // -------------------------
    private void LoadReaction()
    {
        if (currentIndex < 0 || currentIndex >= selectedReactions.Count)
        {
            CompleteLevel();
            return;
        }

        var reaction = selectedReactions[currentIndex];
        if (reaction == null)
        {
            Debug.LogError("[LevelController] ReactionAsset null en selectedReactions");
            CompleteLevel();
            return;
        }

        balanceStation.reaction = reaction;

        if (equationHUD != null)
            equationHUD.SetReaction(reaction);
        // reset llaves del jugador
        if (playerKeyRing != null) playerKeyRing.ClearKeys();

        // reinicia sesión (por si venías de una sesión vieja)
        if (balanceStation != null && balanceStation.session != null)
            balanceStation.session.BindStation(balanceStation); // esto clona coeficientes iniciales

        // configurar fases para la nueva reacción
        phaseManager.ConfigureForReaction(balanceStation, levelConfig.difficulty);

        // activar llaves según fases presentes
        if (keyActivator != null)
            keyActivator.SetActiveKeys(phaseManager.GetPresentPhases());

        if (gateController != null)
            gateController.Configure(phaseManager.GetPresentPhases(), levelConfig.difficulty);

        // volver a exploración
        if (playerRespawner != null && spawnPoint != null)
            playerRespawner.RespawnAt(spawnPoint);

        phase = LevelPhase.Exploration;
        gameMode.EnterExploration();
    }

    public void RequestStartBalance(BalanceStation station)
    {
        if (phase != LevelPhase.Exploration)
        {
            Debug.LogWarning("[LevelController] No se puede iniciar balance: no estás en Exploration");
            return;
        }

        if (station != balanceStation)
        {
            Debug.LogWarning("[LevelController] Station no coincide con balanceStation");
            return;
        }

        phase = LevelPhase.Balance;
        gameMode.EnterBalance(station);
    }
    private void HandleSessionCompleted(BalanceResult result)
    {
        if (scoreByReaction == null || currentIndex < 0 || currentIndex >= scoreByReaction.Length)
        {
            Debug.LogError($"[LevelController] scoreByReaction no coincide. idx={currentIndex} len={(scoreByReaction == null ? -1 : scoreByReaction.Length)}");
            return;
        }

        if (phase != LevelPhase.Balance) return;

        phase = LevelPhase.ReactionCompleted;

        totalTime += result.timeSeconds;
        totalErrors += result.errors;

        // reemplaza score si ya existía para este índice
        int old = scoreByReaction[currentIndex];
        if (old >= 0) totalScore -= old;          // quita el anterior
        scoreByReaction[currentIndex] = result.score;
        totalScore += result.score;

        gameMode.ExitBalance();

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        mapSwitcher?.PushUI();
        resultPanel.Show(result, ResultContext.ReactionCompleted);
    }

    // -------------------------
    // Navegación
    // -------------------------
    public void Continue()
    {
        if (phase == LevelPhase.ReactionCompleted)
        {
            ExitResultsMode();
            currentIndex++;

            if (currentIndex < selectedReactions.Count) LoadReaction();
            else CompleteLevel();

            return;
        }

        if (phase == LevelPhase.LevelCompleted)
        {
            ExitResultsMode();

            if (autoAdvanceToEasy)
            {
                if (GameManager.Instance == null)
                {
                    Debug.LogError("[LevelController] No existe GameManager para avanzar de nivel.");
                    return;
                }

                GameManager.Instance.AdvanceToLevel(easySceneName, easyLevelConfig);
            }
            else
            {
                ActivateNextLevelDoor();
            }

            return;
        }
    }

    private void RetryCurrentReaction()
    {
        if (phase != LevelPhase.ReactionCompleted)
            return;

        ExitResultsMode();

        // HARD RESET REAL (coeficientes a 1 + llaves + respawn)
        RestartCurrentReaction();
    }

    // -------------------------
    // Finalización
    // -------------------------
    private void CompleteLevel()
    {


        phase = LevelPhase.LevelCompleted;

        var summary = new BalanceResult
        {
            timeSeconds = totalTime,
            errors = totalErrors,
            score = totalScore,
            reactionId = ""
        };

        GameManager.Instance?.AddScore(totalScore);

        // ENTRAR A UI/RESULTS MODE
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Si tienes EnterResults que congela jugador/hud
        gameMode?.EnterResults();
        mapSwitcher?.PushUI();
        resultPanel.Show(summary, ResultContext.LevelCompleted);
    }

    void ActivateNextLevelDoor()
    {
        if (nextLevelDoor != null)
            nextLevelDoor.SetActive(true);
    }

    void ExitResultsMode()
    {
        Time.timeScale = 1f;
        if (resultPanel != null) resultPanel.Hide();

        mapSwitcher?.Pop();

        if (gameMode != null) gameMode.EnterExploration(force: true); // <- clave
    }
}