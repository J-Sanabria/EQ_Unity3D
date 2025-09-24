using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("HUD (opcionales)")]
    [SerializeField] TMP_Text txtUser;   // muestra el usuario en pantalla
    [SerializeField] TMP_Text txtScore;  // muestra el puntaje en tiempo real

    public string CurrentUser { get; private set; }
    public int CurrentScore { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Obtén el usuario actual desde UserDB
        if (UserDB.Instance != null)
            CurrentUser = UserDB.Instance.GetCurrentUser();

        if (txtUser) txtUser.text = string.IsNullOrEmpty(CurrentUser) ? "-" : CurrentUser;
        UpdateScoreHUD();
    }

    void UpdateScoreHUD()
    {
        if (txtScore) txtScore.text = CurrentScore.ToString();
    }

    // Llama esto cuando el jugador gane puntos
    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        CurrentScore += amount;
        UpdateScoreHUD();
    }

    // Llama esto al terminar la partida
    public void FinishGame()
    {
        if (!string.IsNullOrEmpty(CurrentUser) && UserDB.Instance != null)
            UserDB.Instance.RecordScore(CurrentUser, CurrentScore);

        Debug.Log($"Puntaje guardado: {CurrentUser} -> {CurrentScore}");

        // Aquí puedes cargar pantalla de resultados o volver al menú:
        // SceneManager.LoadScene("Resultados");
        // o
        // SceneManager.LoadScene("Menu");
    }
}
