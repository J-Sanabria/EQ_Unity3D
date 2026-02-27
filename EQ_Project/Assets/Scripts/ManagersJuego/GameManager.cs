using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Flow")]
    [SerializeField] LevelConfig initialLevel;

    public string CurrentUser { get; private set; }
    public int CurrentScore { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (UserDB.Instance != null)
            CurrentUser = UserDB.Instance.GetCurrentUser();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Arranque inicial (en la escena actual)
        StartCoroutine(StartLevelWhenReady());
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si cambias de escena en el futuro, reinicia el flujo aquí también
        StartCoroutine(StartLevelWhenReady());
    }

    IEnumerator StartLevelWhenReady()
    {
        // espera 1 frame para que todo haga Awake/OnEnable
        yield return null;

        if (initialLevel == null || initialLevel.reactionPool == null)
        {
            Debug.LogError("[GameManager] initialLevel inválido o sin reactionPool asignado");
            yield break;
        }

        // Busca el LevelController de la escena activa
        var lc = Object.FindFirstObjectByType<LevelController>();
        if (lc == null)
        {
            Debug.LogError("[GameManager] No encontré LevelController en la escena activa");
            yield break;
        }

        lc.StartLevel(initialLevel);
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        CurrentScore += amount;
    }

    public void FinishGame()
    {
        if (!string.IsNullOrEmpty(CurrentUser) && UserDB.Instance != null)
            UserDB.Instance.RecordScore(CurrentUser, CurrentScore);

        Debug.Log($"Puntaje final guardado: {CurrentUser} -> {CurrentScore}");
    }
}