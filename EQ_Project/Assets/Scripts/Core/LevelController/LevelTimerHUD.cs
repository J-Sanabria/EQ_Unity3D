using CB.Balance;
using TMPro;
using UnityEngine;

public class LevelTimerHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BalanceStation balanceStation;

    [Header("UI")]
    [SerializeField] private TMP_Text[] timerTexts;

    [Header("Behavior")]
    [SerializeField] private bool hideWhenNoSession = false;
    [SerializeField] private string noSessionText = "--:--";

    void Update()
    {
        float timeValue;
        bool hasTime = TryGetEquationTime(out timeValue);

        for (int i = 0; i < timerTexts.Length; i++)
        {
            TMP_Text txt = timerTexts[i];
            if (txt == null) continue;

            if (!hasTime)
            {
                if (hideWhenNoSession)
                    txt.gameObject.SetActive(false);
                else
                {
                    txt.gameObject.SetActive(true);
                    txt.text = noSessionText;
                }

                continue;
            }

            txt.gameObject.SetActive(true);

            int minutes = Mathf.FloorToInt(timeValue / 60f);
            int seconds = Mathf.FloorToInt(timeValue % 60f);
            txt.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private bool TryGetEquationTime(out float timeValue)
    {
        timeValue = 0f;

        if (balanceStation == null || balanceStation.session == null)
            return false;

        if (!balanceStation.session.HasStartedOnce)
            return false;

        timeValue = balanceStation.session.elapsed;
        return true;
    }
}