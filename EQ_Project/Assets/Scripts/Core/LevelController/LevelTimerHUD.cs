using TMPro;
using UnityEngine;

public class LevelTimerHUD : MonoBehaviour
{
    [SerializeField] private LevelController levelController;
    [SerializeField] private TMP_Text timerText;

    void Update()
    {
        if (levelController == null || timerText == null) return;

        float t = levelController.GetLevelElapsedTime();
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}