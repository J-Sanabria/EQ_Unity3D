using TMPro;
using UnityEngine;

public class ProfessorRowUI : MonoBehaviour
{
    public TMP_Text txtName;
    public TMP_Text txtBest;
    public TMP_Text txtAvg;
    public TMP_Text txtGames;
    public TMP_Text txtLast;

    public void Bind(UserScoreData u)
    {
        txtName.text = u.name;
        txtBest.text = u.bestScore.ToString();
        txtAvg.text = u.avgScore.ToString("0.0");
        txtGames.text = u.gamesPlayed.ToString();

        txtLast.text = string.IsNullOrEmpty(u.lastPlayedISO)
            ? "-"
            : System.DateTime.Parse(u.lastPlayedISO).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }
}
