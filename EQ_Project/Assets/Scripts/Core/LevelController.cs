using System.Collections.Generic;
using UnityEngine;
using CB.Balance;
using CB.Core;

public enum Difficulty
{
    Tutorial,
    Easy,
    Medium,
    Hard
}

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
    [SerializeField] LevelConfig levelConfig;

    [Header("Refs")]
    [SerializeField] BalanceStation balanceStation;
    [SerializeField] GameModeController gameMode;
    [SerializeField] BalanceResultPanel resultPanel;
    [SerializeField] EquationHUDBinding equationHUD;
    [SerializeField] SpawnerConfigurator spawnerConfigurator;

    LevelPhase phase = LevelPhase.Exploration;

    List<ReactionAsset> selectedReactions = new();
    int currentIndex = -1;

    // métricas acumuladas
    float totalTime;
    int totalErrors;
    int totalScore;

    void Awake()
    {
        if (resultPanel != null)
        {
            resultPanel.OnContinueRequested += Continue;
            resultPanel.OnRetryRequested += RetryCurrentReaction;
        }
    }

    // -------------------------
    // Inicio de nivel
    // -------------------------
    public void StartLevel(LevelConfig config)
    {
        Debug.Log("Estoy entrando a StartLevel");
        
        if (config == null || config.reactionPool == null)
        {
            Debug.LogError("LevelConfig inválido");
            return;
        }

        levelConfig = config;

        selectedReactions = PickRandomReactions(
            config.reactionPool.reactions,
            config.reactionsPerRun
        );

        if (selectedReactions.Count == 0)
        {
            Debug.LogError("No hay reacciones seleccionadas");
            return;
        }

        currentIndex = 0;
        totalTime = 0f;
        totalErrors = 0;
        totalScore = 0;

        LoadReaction();
    }

    void OnEnable()
    {
        if (balanceStation != null && balanceStation.session != null)
            balanceStation.session.OnSessionCompleted += HandleSessionCompleted;
    }

    void OnDisable()
    {
        if (balanceStation != null && balanceStation.session != null)
            balanceStation.session.OnSessionCompleted -= HandleSessionCompleted;
    }

    void HandleSessionCompleted(BalanceResult result)
    {
        if (phase != LevelPhase.Balance)
            return;

        phase = LevelPhase.ReactionCompleted;

        totalTime += result.timeSeconds;
        totalErrors += result.errors;
        totalScore += result.score;

        // salir del modo Balance
        gameMode.ExitBalance();

        resultPanel.Show(result, ResultContext.ReactionCompleted);
    }

    // -------------------------
    // Selección de reacciones
    // -------------------------
    List<ReactionAsset> PickRandomReactions(
        List<ReactionAsset> source,
        int count)
    {
        if (source == null || source.Count == 0 || count <= 0)
        {
            Debug.LogError("ReactionPool inválido");
            return new List<ReactionAsset>();
        }

        if (source.Count < count)
        {
            Debug.LogError("No hay suficientes reacciones en el pool");
            return new List<ReactionAsset>(source);
        }

        List<ReactionAsset> copy = new(source);
        List<ReactionAsset> result = new();

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
    void LoadReaction()
    {
        if (currentIndex < 0 || currentIndex >= selectedReactions.Count)
        {
            CompleteLevel();
            return;
        }

        ReactionAsset reaction = selectedReactions[currentIndex];
        balanceStation.reaction = reaction;

        spawnerConfigurator?.ConfigureForReaction(reaction);

        if (equationHUD != null)
            equationHUD.SetReaction(reaction);

        phase = LevelPhase.Exploration;
        gameMode.EnterExploration();
    }

    public void StartBalancePhase()
    {
        if (phase != LevelPhase.Exploration)
            return;


        phase = LevelPhase.Balance;

    }

    public void OnReactionSolved(BalanceResult result)
    {
        if (phase != LevelPhase.Balance)
            return;

        phase = LevelPhase.ReactionCompleted;

        totalTime += result.timeSeconds;
        totalErrors += result.errors;
        totalScore += result.score;

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

        if (currentIndex < selectedReactions.Count)
        {
            LoadReaction();
        }
        else
        {
            CompleteLevel();
        }
    }

    void RetryCurrentReaction()
    {
        if (phase != LevelPhase.ReactionCompleted)
            return;

        LoadReaction();
    }

    // -------------------------
    // Finalización
    // -------------------------
    void CompleteLevel()
    {
        phase = LevelPhase.LevelCompleted;

        BalanceResult summary = new BalanceResult
        {
            timeSeconds = totalTime,
            errors = totalErrors,
            score = totalScore
        };

        GameManager.Instance?.AddScore(totalScore);

        resultPanel.Show(summary, ResultContext.LevelCompleted);
    }

    public void RequestStartBalance(BalanceStation station)
    {
        Debug.Log("Entro a levelController");
        if (phase != LevelPhase.Exploration)
            return;
        Debug.Log("No es la fase de exploración");

        if (station != balanceStation)
            return;
        Debug.Log("No es la balanza");

        phase = LevelPhase.Balance;
        gameMode.EnterBalance(station);

     
    }
}
