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
    [SerializeField] private PhaseGateController gateController;
    [SerializeField] private ActionMapSwitcher mapSwitcher;
    [SerializeField] private GameObject nextLevelDoor;

    [Header("Next Level")]
    [SerializeField] private string easySceneName = "Easy";
    [SerializeField] private LevelConfig easyLevelConfig;
    [SerializeField] private bool autoAdvanceToEasy = true;

    private float levelElapsedTime;
    private bool levelTimerRunning;
    private LevelPhase phase = LevelPhase.Exploration;

    private List<ReactionAsset> selectedReactions = new();
    private int currentIndex = -1;
    private int[] scoreByReaction;

    // métricas acumuladas
    private float totalReactionTime;
    private int totalErrors;
    private int totalScore;

#if UNITY_EDITOR
    [Header("Editor Only - Auto Start (para probar sin menú)")]
    [SerializeField] private bool editorAutoStart = true;
    [SerializeField] private string editorUser = "TestUser";
#endif

    private void Awake()
    {
        if (resultPanel != null)
        {
            resultPanel.OnContinueRequested += Continue;
            resultPanel.OnRetryRequested += RetryCurrentReaction;
        }
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (!editorAutoStart) return;

        // Si GameManager ya pidió iniciar desde menú, no hagas auto start local.
        if (GameManager.Instance != null && GameManager.Instance.WillAutoStartLevel())
            return;

        if (levelConfig == null)
        {
            Debug.LogWarning("[LevelController] EditorAutoStart: falta LevelConfig asignado en el inspector.");
            return;
        }

        if (UserDB.Instance != null && string.IsNullOrEmpty(UserDB.Instance.GetCurrentUser()))
            UserDB.Instance.SetCurrentUser(editorUser);

        StartLevel(levelConfig);
#endif
    }

    private void Update()
    {
        if (levelTimerRunning)
            levelElapsedTime += Time.deltaTime;
    }

    private void OnEnable()
    {
        if (balanceStation != null && balanceStation.session != null)
            balanceStation.session.OnSessionCompleted += HandleSessionCompleted;

        if (gameMode != null)
            gameMode.OnStateChanged += HandleGameModeState;
    }

    private void OnDisable()
    {
        if (balanceStation != null && balanceStation.session != null)
            balanceStation.session.OnSessionCompleted -= HandleSessionCompleted;

        if (gameMode != null)
            gameMode.OnStateChanged -= HandleGameModeState;
    }

    private void HandleGameModeState(GameState state)
    {
        if (state == GameState.Exploration && phase == LevelPhase.Balance)
            phase = LevelPhase.Exploration;
    }

    private void EnsureScoreArray()
    {
        if (selectedReactions == null) return;

        scoreByReaction = new int[selectedReactions.Count];
        for (int i = 0; i < scoreByReaction.Length; i++)
            scoreByReaction[i] = -1;
    }

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

        if (phaseManager == null)
        {
            Debug.LogError("[LevelController] Falta PhaseManager asignado");
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
            Debug.LogWarning("[LevelController] equationHUD no asignado.");

        EnsureScoreArray();

        currentIndex = 0;
        totalReactionTime = 0f;
        totalErrors = 0;
        totalScore = 0;

        levelElapsedTime = 0f;
        levelTimerRunning = true;
        phase = LevelPhase.Exploration;

        LoadReaction();
    }

    public float GetLevelElapsedTime()
    {
        return levelElapsedTime;
    }

    public void RestartCurrentReaction()
    {
        if (balanceStation == null || balanceStation.reaction == null)
            return;

        if (playerKeyRing != null)
            playerKeyRing.ClearKeys();

        if (balanceStation.session != null)
        {
            balanceStation.session.BindStation(balanceStation);
            balanceStation.session.ResetCoefsToOnes();
        }

        if (equationHUD != null)
            equationHUD.SetReaction(balanceStation.reaction);

        if (phaseManager != null)
            phaseManager.ConfigureForReaction(balanceStation, levelConfig.difficulty);

        if (keyActivator != null)
            keyActivator.SetActiveKeys(phaseManager.GetPresentPhases());

        if (gateController != null)
            gateController.Configure(phaseManager.GetPresentPhases(), levelConfig.difficulty);

        if (playerRespawner != null && spawnPoint != null)
            playerRespawner.RespawnAt(spawnPoint);

        phase = LevelPhase.Exploration;
        gameMode.EnterExploration();

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

        if (playerKeyRing != null)
            playerKeyRing.ClearKeys();

        if (balanceStation != null && balanceStation.session != null)
            balanceStation.session.BindStation(balanceStation);

        phaseManager.ConfigureForReaction(balanceStation, levelConfig.difficulty);

        if (keyActivator != null)
            keyActivator.SetActiveKeys(phaseManager.GetPresentPhases());

        if (gateController != null)
            gateController.Configure(phaseManager.GetPresentPhases(), levelConfig.difficulty);

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

        if (phase != LevelPhase.Balance)
            return;

        phase = LevelPhase.ReactionCompleted;

        totalReactionTime += result.timeSeconds;
        totalErrors += result.errors;

        int old = scoreByReaction[currentIndex];
        if (old >= 0)
            totalScore -= old;

        scoreByReaction[currentIndex] = result.score;
        totalScore += result.score;

        gameMode.ExitBalance();

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        mapSwitcher?.PushUI();
        resultPanel.Show(result, ResultContext.ReactionCompleted);
    }

    public void Continue()
    {
        if (phase == LevelPhase.ReactionCompleted)
        {
            ExitResultsMode();
            currentIndex++;

            if (currentIndex < selectedReactions.Count)
                LoadReaction();
            else
                CompleteLevel();

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
        }
    }

    private void RetryCurrentReaction()
    {
        if (phase != LevelPhase.ReactionCompleted)
            return;

        ExitResultsMode();
        RestartCurrentReaction();
    }

    private void CompleteLevel()
    {
        levelTimerRunning = false;
        phase = LevelPhase.LevelCompleted;

        var summary = new BalanceResult
        {
            timeSeconds = levelElapsedTime,
            errors = totalErrors,
            score = totalScore,
            reactionId = ""
        };

        GameManager.Instance?.AddScore(totalScore);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        gameMode?.EnterResults();
        mapSwitcher?.PushUI();
        resultPanel.Show(summary, ResultContext.LevelCompleted);
    }

    private void ActivateNextLevelDoor()
    {
        if (nextLevelDoor != null)
            nextLevelDoor.SetActive(true);
    }

    private void ExitResultsMode()
    {
        Time.timeScale = 1f;

        if (resultPanel != null)
            resultPanel.Hide();

        mapSwitcher?.Pop();
        gameMode?.EnterExploration(force: true);
    }
}