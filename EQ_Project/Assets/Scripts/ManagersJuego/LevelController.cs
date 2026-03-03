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

    private LevelPhase phase = LevelPhase.Exploration;

    private List<ReactionAsset> selectedReactions = new();
    private int currentIndex = -1;

    // métricas acumuladas
    private float totalTime;
    private int totalErrors;
    private int totalScore;

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


        currentIndex = 0;
        totalTime = 0f;
        totalErrors = 0;
        totalScore = 0;

        LoadReaction();
    }

    public void RestartCurrentReaction()
    {
        // Resetea la reacción actual sin cambiar currentIndex
        // Esto fuerza: llaves, sesión, fases, respawn, exploración.
        LoadReaction();
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
        if (phase != LevelPhase.Balance)
            return;

        phase = LevelPhase.ReactionCompleted;

        totalTime += result.timeSeconds;
        totalErrors += result.errors;
        totalScore += result.score;

        // salir de balance
        gameMode.ExitBalance();

        // pausar y mostrar cursor
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        resultPanel.Show(result, ResultContext.ReactionCompleted);
    }

    // -------------------------
    // Navegación
    // -------------------------
    public void Continue()
    {
        if (phase != LevelPhase.ReactionCompleted)
            return;

        ExitResultsMode();

        currentIndex++;

        if (currentIndex < selectedReactions.Count)
        {
            LoadReaction();
        }
        else
        {
            CompleteLevel();
        }
    }

    private void RetryCurrentReaction()
    {
        if (phase != LevelPhase.ReactionCompleted)
            return;

        ExitResultsMode();

        // reinicia la sesión de la misma reacción
        // IMPORTANTE: no basta con LoadReaction si tu player tiene llaves / está en balanza
        LoadReaction();
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
            reactionId = "" // opcional: o "LEVEL_SUMMARY"
        };

        GameManager.Instance?.AddScore(totalScore);
        resultPanel.Show(summary, ResultContext.LevelCompleted);
    }

    void ExitResultsMode()
    {
        // 1) reanuda tiempo
        Time.timeScale = 1f;

        // 2) oculta panel
        if (resultPanel != null) resultPanel.Hide();

        // 3) vuelve a exploración (reactiva movimiento/inputs)
        if (gameMode != null) gameMode.EnterExploration();
    }
}