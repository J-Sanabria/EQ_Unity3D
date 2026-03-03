using UnityEngine;
using System.Linq;

public class ProfessorPanelController : MonoBehaviour
{
    [SerializeField] Transform content;
    [SerializeField] GameObject rowPrefab;

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
        if (UserDB.Instance == null || content == null || rowPrefab == null) return;

        ClearContent();
        var list = UserDB.Instance.GetAllUsers();

        list = sort switch
        {
            SortMode.Best => list.OrderByDescending(u => u.bestScore).ToList(),
            SortMode.Average => list.OrderByDescending(u => u.avgScore).ToList(),
            _ => list.OrderBy(u => u.name).ToList()
        };

        foreach (var u in list)
        {
            var row = Instantiate(rowPrefab, content);
            var ui = row.GetComponent<ProfessorRowUI>();
            if (ui != null) ui.Bind(u);
            Debug.Log($"Row spawned. Has ProfessorRowUI? {row.GetComponent<ProfessorRowUI>() != null}");
        }

        Debug.Log($"Users: {list.Count}");
    }
}