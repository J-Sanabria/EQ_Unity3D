using System.Collections.Generic;
using UnityEngine;
using CB.Balance;
using CB.Core;

public enum Difficulty { Tutorial, Easy, Medium, Hard }

public enum LevelPhase
{
    Exploration,
    Balance,
    ReactionCompleted,
    LevelCompleted,
    GameCompleted
}

[CreateAssetMenu(menuName = "ChemicalBalance/ReactionPool")]
public class ReactionPool : ScriptableObject
{
    public Difficulty difficulty;
    public List<ReactionAsset> reactions; // ej: 6
}

[CreateAssetMenu(menuName = "ChemicalBalance/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public Difficulty difficulty;
    public ReactionPool reactionPool;
    public int reactionsPerRun = 3;
}

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

        currentIndex = 0;
        totalTime = 0f;
        totalErrors = 0;
        totalScore = 0;

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

        phaseManager.ConfigureForReaction(balanceStation, levelConfig.difficulty);

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

        gameMode.ExitBalance();
        resultPanel.Show(result, ResultContext.ReactionCompleted);
    }

    // -------------------------
    // Navegación
    // -------------------------
    public void Continue()
    {
        if (phase != LevelPhase.ReactionCompleted)
            return;

        currentIndex++;

        if (currentIndex < selectedReactions.Count) LoadReaction();
        else CompleteLevel();
    }

    private void RetryCurrentReaction()
    {
        if (phase != LevelPhase.ReactionCompleted)
            return;

        // Recarga la misma reacción y vuelve a exploration limpio
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
}