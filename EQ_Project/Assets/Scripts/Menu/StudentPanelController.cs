using UnityEngine;
using TMPro;
using UnityEditor;

public class StudentPanelController : MonoBehaviour
{
    [SerializeField] TMP_InputField inpNombre;
    [SerializeField] TMP_Dropdown ddUsuarios;
    [SerializeField] MenuManager menu;

    void OnEnable() => RefreshUsers();

    void RefreshUsers()
    {
        if (UserDB.Instance == null) { Debug.LogWarning("UserDB aún no está listo."); return; }
        if (ddUsuarios == null) { Debug.LogError("DdUsuarios no asignado en Inspector."); return; }

        var users = UserDB.Instance.GetAllUsers();
        ddUsuarios.ClearOptions();
        var opts = new System.Collections.Generic.List<string>();
        foreach (var u in users) opts.Add(u.name);
        ddUsuarios.AddOptions(opts);
        ddUsuarios.RefreshShownValue();
    }

    // Botón: Crear
    // Crear
    public void OnClick_CreateUser()
    {
        var name = inpNombre.text.Trim();
        if (string.IsNullOrEmpty(name)) { Debug.Log("Nombre vacío."); return; }
        if (!UserDB.Instance.AddUser(name)) { Debug.Log("Ya existe o inválido."); return; }
        UserDB.Instance.SetCurrentUser(name);
        RefreshUsers();
        menu.OnStudentPicked(); // <-- abre confirmación o entra directo
    }

    // Seleccionar
    public void OnClick_SelectUser()
    {
        if (ddUsuarios.options.Count == 0) return;
        string name = ddUsuarios.options[ddUsuarios.value].text;
        UserDB.Instance.SetCurrentUser(name);
        menu.OnStudentPicked(); // <-- abre confirmación o entra directo
    }
}
