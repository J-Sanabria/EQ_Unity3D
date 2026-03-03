using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StudentSelectPanelController : MonoBehaviour
{
    [SerializeField] TMP_Dropdown ddUsuarios;
    [SerializeField] MenuManager menu;

    void OnEnable() => RefreshUsers();

    public void RefreshUsers()
    {
        if (UserDB.Instance == null) return;
        if (ddUsuarios == null) return;

        var users = UserDB.Instance.GetAllUsers();
        ddUsuarios.ClearOptions();

        var opts = new List<string>();
        foreach (var u in users) opts.Add(u.name);

        ddUsuarios.AddOptions(opts);
        ddUsuarios.RefreshShownValue();
    }

    public void OnClick_SelectUser()
    {
        if (UserDB.Instance == null) return;
        if (ddUsuarios == null) return;
        if (ddUsuarios.options.Count == 0) return;

        string name = ddUsuarios.options[ddUsuarios.value].text;
        UserDB.Instance.SetCurrentUser(name);

        menu.OnStudentPicked();
    }

    public void OnClick_OpenCreatePanel()
    {
        if (menu != null) menu.OnClick_IrACrearUsuario();
    }
}