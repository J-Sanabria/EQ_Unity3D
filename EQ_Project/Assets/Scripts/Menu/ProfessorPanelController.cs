using UnityEngine;
using TMPro;
using System.Linq;

public class ProfessorPanelController : MonoBehaviour
{
    [SerializeField] Transform content;     // Content del ScrollView
    [SerializeField] GameObject rowPrefab;  // Prefab con textos

    enum SortMode { Best, Average, Name }
    SortMode sort = SortMode.Best;

    void OnEnable() => Refresh();

    public void SortByBest() { sort = SortMode.Best; Refresh(); }
    public void SortByAverage() { sort = SortMode.Average; Refresh(); }
    public void SortByName() { sort = SortMode.Name; Refresh(); }

    void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
    }

    void Refresh()
    {
        if (UserDB.Instance == null) { Debug.LogWarning("UserDB aún no está listo."); return; }
        if (content == null) { Debug.LogError("Content no asignado."); return; }
        if (rowPrefab == null) { Debug.LogError("RowPrefab no asignado."); return; }

        ClearContent();
        var list = UserDB.Instance.GetAllUsers();

        switch (sort)
        {
            case SortMode.Best: list = list.OrderByDescending(u => u.bestScore).ToList(); break;
            case SortMode.Average: list = list.OrderByDescending(u => u.avgScore).ToList(); break;
            case SortMode.Name: list = list.OrderBy(u => u.name).ToList(); break;
        }

        foreach (var u in list)
        {
            var row = Instantiate(rowPrefab, content);
            var texts = row.GetComponentsInChildren<TMP_Text>();
            // Asume el orden: 0 Nombre, 1 Best, 2 Avg, 3 Games, 4 Last
            texts[0].text = u.name;
            texts[1].text = u.bestScore.ToString();
            texts[2].text = u.avgScore.ToString("0.0");
            texts[3].text = u.gamesPlayed.ToString();
            texts[4].text = string.IsNullOrEmpty(u.lastPlayedISO) ? "-" : System.DateTime.Parse(u.lastPlayedISO).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        }
    }
}
