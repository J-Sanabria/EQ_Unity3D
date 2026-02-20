using CB.Balance;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ResultContext
{
    ReactionCompleted,
    LevelCompleted,
    GameCompleted
}

public class BalanceResultPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text txtTitulo;
    [SerializeField] TMP_Text txtDetalle;
    [SerializeField] TMP_Text txtScore;
    [SerializeField] Button btnContinuar;
    [SerializeField] Button btnReintentar;

    public System.Action OnContinueRequested;
    public System.Action OnRetryRequested;

    void Awake()
    {
        if (btnContinuar != null)
            btnContinuar.onClick.AddListener(() => OnContinueRequested?.Invoke());

        if (btnReintentar != null)
            btnReintentar.onClick.AddListener(() => OnRetryRequested?.Invoke());
    }

    public void Show(BalanceResult result, ResultContext context)
    {
        gameObject.SetActive(true);

        switch (context)
        {
            case ResultContext.ReactionCompleted:
                txtTitulo.text = "¡Ecuación balanceada!";
                break;

            case ResultContext.LevelCompleted:
                txtTitulo.text = "¡Nivel completado!";
                break;

            case ResultContext.GameCompleted:
                txtTitulo.text = "¡Juego completado!";
                break;
        }

        txtDetalle.text =
            $"Tiempo: {Mathf.RoundToInt(result.timeSeconds)} s   Errores: {result.errors}";

        txtScore.text = $"Puntaje: {result.score}";

        // Visibilidad de botones según contexto
        btnReintentar.gameObject.SetActive(context == ResultContext.ReactionCompleted);
        btnContinuar.gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
