using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Flow")]
    [SerializeField] LevelController levelController;
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
    }

    void Start()
    {
        if (levelController == null || initialLevel == null)
        {
            Debug.LogError("GameManager mal configurado: falta LevelController o LevelConfig");
            return;
        }

        levelController.StartLevel(initialLevel);
    }

    // -------------------------
    // Score
    // -------------------------
    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        CurrentScore += amount;
    }

    // -------------------------
    // Fin de juego
    // -------------------------
    public void FinishGame()
    {
        if (!string.IsNullOrEmpty(CurrentUser) && UserDB.Instance != null)
            UserDB.Instance.RecordScore(CurrentUser, CurrentScore);

        Debug.Log($"Puntaje final guardado: {CurrentUser} -> {CurrentScore}");

        // Aquí decides el flujo:
        // SceneManager.LoadScene("Menu");
        // SceneManager.LoadScene("Resultados");
    }
}
