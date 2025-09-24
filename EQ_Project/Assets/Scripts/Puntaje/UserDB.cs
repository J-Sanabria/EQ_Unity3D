using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserScoreData
{
    public string name;
    public List<int> scores = new List<int>();
    public int gamesPlayed => scores.Count;
    public int bestScore => scores.Count > 0 ? scores.Max() : 0;
    public float avgScore => scores.Count > 0 ? (float)scores.Average() : 0f;
    public string lastPlayedISO; // DateTime.UtcNow.ToString("o")
}

[Serializable]
public class UserDatabase
{
    public List<UserScoreData> users = new List<UserScoreData>();
}

public class UserDB : MonoBehaviour
{
    public static UserDB Instance { get; private set; }
    const string FILE_NAME = "users.json";
    const string PREF_CURRENT_USER = "current_user";

    string FilePath => Path.Combine(Application.persistentDataPath, FILE_NAME);
    UserDatabase db = new UserDatabase();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ---------- Persistencia ----------
    public void Load()
    {
        try
        {
            if (File.Exists(FilePath))
                db = JsonUtility.FromJson<UserDatabase>(File.ReadAllText(FilePath));
            if (db == null) db = new UserDatabase();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"UserDB Load error: {e.Message}");
            db = new UserDatabase();
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonUtility.ToJson(db, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"UserDB Save error: {e.Message}");
        }
    }

    // ---------- Usuarios ----------
    public bool Exists(string name) => db.users.Any(u => string.Equals(u.name, name, StringComparison.OrdinalIgnoreCase));

    public bool AddUser(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || Exists(name)) return false;
        db.users.Add(new UserScoreData { name = name.Trim(), lastPlayedISO = "" });
        Save();
        return true;
    }

    public bool RemoveUser(string name)
    {
        var u = db.users.FirstOrDefault(x => x.name == name);
        if (u == null) return false;
        db.users.Remove(u);
        Save();
        return true;
    }

    public IReadOnlyList<UserScoreData> GetAllUsers() => db.users;

    public UserScoreData GetUser(string name) => db.users.FirstOrDefault(u => u.name == name);

    // ---------- Puntajes ----------
    public void RecordScore(string name, int score)
    {
        var u = GetUser(name);
        if (u == null) { Debug.LogWarning("RecordScore: usuario no existe."); return; }
        u.scores.Add(Mathf.Max(0, score));
        u.lastPlayedISO = DateTime.UtcNow.ToString("o");
        Save();
    }

    // ---------- Usuario actual ----------
    public void SetCurrentUser(string name) => PlayerPrefs.SetString(PREF_CURRENT_USER, name);
    public string GetCurrentUser() => PlayerPrefs.GetString(PREF_CURRENT_USER, "");
    public bool HasCurrentUser() => !string.IsNullOrEmpty(GetCurrentUser());
}
