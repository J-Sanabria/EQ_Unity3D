using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Default Level")]
    [SerializeField] LevelConfig initialLevel;

    bool _progressSavedThisRun;

    public string CurrentUser { get; private set; }
    public int CurrentScore { get; private set; }

    public bool WillAutoStartLevel() => _shouldAutoStartLevel;

    // Flag: solo auto-start si venimos de “StartGame”
    bool _shouldAutoStartLevel;

    void Awake()
    {

        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        // NO leas CurrentUser aquí como “verdad”.
        // El menú define el usuario y debe llamarte a SetCurrentUser.
        CurrentUser = UserDB.Instance != null ? UserDB.Instance.GetCurrentUser() : "";
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetCurrentUser(string user)
    {
        CurrentUser = string.IsNullOrWhiteSpace(user) ? "" : user.Trim();
        if (UserDB.Instance != null) UserDB.Instance.SetCurrentUser(CurrentUser);
    }

    public void StartNewGame(string sceneName, LevelConfig levelConfig)
    {
        if (string.IsNullOrEmpty(CurrentUser))
        {
            Debug.LogWarning("[GameManager] StartNewGame: no hay usuario actual.");
            return;
        }

        if (levelConfig == null || levelConfig.reactionPool == null)
        {
            Debug.LogError("[GameManager] StartNewGame: LevelConfig inválido.");
            return;
        }

        initialLevel = levelConfig;

        CurrentScore = 0;
        _progressSavedThisRun = false; // importante: reinicia guardado para esta run

        _shouldAutoStartLevel = true;
        SceneManager.LoadScene(sceneName);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_shouldAutoStartLevel) return;

        // Intenta arrancar solo si existe LevelController
        StartCoroutine(StartLevelWhenReady());
    }

    public void SaveProgressSoFar()
    {
        if (_progressSavedThisRun) return; // evita guardar duplicado si el usuario spamea salir
        _progressSavedThisRun = true;

        if (string.IsNullOrEmpty(CurrentUser) || UserDB.Instance == null) return;

        // Guarda el score acumulado hasta ahora
        UserDB.Instance.RecordScore(CurrentUser, CurrentScore);
        Debug.Log($"[GameManager] Progreso guardado: {CurrentUser} -> {CurrentScore}");
    }

    IEnumerator StartLevelWhenReady()
    {
        yield return null;

        var lc = Object.FindFirstObjectByType<LevelController>();
        if (lc == null)
        {
            // Esta escena no es gameplay. No hagas nada.
            _shouldAutoStartLevel = false;
            yield break;
        }

        if (initialLevel == null || initialLevel.reactionPool == null)
        {
            Debug.LogError("[GameManager] initialLevel inválido al cargar gameplay.");
            _shouldAutoStartLevel = false;
            yield break;
        }

        lc.StartLevel(initialLevel);

        // Ya arrancó; si vuelves a cargar otra escena gameplay desde menú, StartNewGame vuelve a activar esto.
        _shouldAutoStartLevel = false;
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        CurrentScore += amount;
    }

    public void AdvanceToLevel(string sceneName, LevelConfig nextLevelConfig)
    {
        if (nextLevelConfig == null || nextLevelConfig.reactionPool == null)
        {
            Debug.LogError("[GameManager] AdvanceToLevel: LevelConfig inválido.");
            return;
        }

        // Mantiene CurrentUser y CurrentScore
        initialLevel = nextLevelConfig;

        _shouldAutoStartLevel = true;
        SceneManager.LoadScene(sceneName);
    }

    public void FinishGame()
    {
        if (!string.IsNullOrEmpty(CurrentUser) && UserDB.Instance != null)
            UserDB.Instance.RecordScore(CurrentUser, CurrentScore);

        Debug.Log($"[GameManager] Puntaje final guardado: {CurrentUser} -> {CurrentScore}");
    }
}