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

    [Header("UI Navigation")]
    [SerializeField] private UIMenuSelectionDriver uiSelectionDriver;
    [SerializeField] private GameObject firstReactionCompletedSelected;
    [SerializeField] private GameObject firstLevelCompletedSelected;
    [SerializeField] private GameObject firstGameCompletedSelected;

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

        if (context == ResultContext.ReactionCompleted)
        {
            if (result.isTutorial)
            {
                txtDetalle.text =
                    $"Tutorial completado\n" +
                    $"Tiempo: {Mathf.RoundToInt(result.timeSeconds)} s\n" +
                    $"Errores: {result.errors}\n" +
                    $"Pasos usados: {result.stepsUsed}";
            }
            else
            {
                txtDetalle.text =
                    $"Tiempo: {Mathf.RoundToInt(result.timeSeconds)} s / {Mathf.RoundToInt(result.targetTimeSeconds)} s\n" +
                    $"Errores: {result.errors} (-{result.penaltyErrors})\n" +
                    $"Pasos: {result.stepsUsed} / {result.idealSteps}  | margen: +{result.freeExtraSteps}\n" +
                    $"Pasos extra penalizados: {result.extraSteps} (-{result.penaltySteps})\n" +
                    $"Penalización por tiempo: -{result.penaltyTime}";
            }
        }
        else
        {
            txtDetalle.text =
                $"Tiempo: {Mathf.RoundToInt(result.timeSeconds)} s   Errores: {result.errors}";
        }

        txtScore.text = $"Puntaje: {result.score}";

        // Visibilidad de botones según contexto
        btnReintentar.gameObject.SetActive(context == ResultContext.ReactionCompleted);
        btnContinuar.gameObject.SetActive(true);

        GameObject firstSelected = null;

        switch (context)
        {
            case ResultContext.ReactionCompleted:
                firstSelected = firstReactionCompletedSelected;
                break;

            case ResultContext.LevelCompleted:
                firstSelected = firstLevelCompletedSelected;
                break;

            case ResultContext.GameCompleted:
                firstSelected = firstGameCompletedSelected;
                break;
        }

        uiSelectionDriver?.SetFirstSelected(firstSelected, clearCurrentSelection: true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
