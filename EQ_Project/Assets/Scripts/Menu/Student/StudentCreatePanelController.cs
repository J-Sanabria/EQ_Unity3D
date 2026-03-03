using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentCreatePanelController : MonoBehaviour
{
    [SerializeField] TMP_InputField inpNombre;
    [SerializeField] Button btnCrear;

    [Header("Opcional: para refrescar dropdown al volver")]
    [SerializeField] StudentSelectPanelController selectPanel;
    [SerializeField] MenuManager menu;

    void OnEnable()
    {
        if (inpNombre != null)
        {
            inpNombre.text = "";
            inpNombre.onValueChanged.RemoveAllListeners();
            inpNombre.onValueChanged.AddListener(_ => Validate());
        }

        Validate();
    }

    void Validate()
    {
        string name = inpNombre != null ? inpNombre.text.Trim() : "";
        bool ok = IsNameValid(name);

        if (btnCrear != null)
            btnCrear.interactable = ok;
    }

    bool IsNameValid(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (name.Length < 3) return false;          // ajusta si quieres
        if (name.Length > 18) return false;         // ajusta si quieres

        // opcional: evitar caracteres raros
        // si quieres permitir espacios, cambia la condición
        foreach (char c in name)
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                return false;

        return true;
    }

    public void OnClick_CreateUser()
    {
        if (UserDB.Instance == null) return;

        var name = inpNombre.text.Trim();
        if (!IsNameValid(name)) return;

        if (!UserDB.Instance.AddUser(name))
        {
            Debug.Log("Ya existe o es inválido.");
            return;
        }

        UserDB.Instance.SetCurrentUser(name);

        // refresca dropdown del panel A
        if (selectPanel != null)
            selectPanel.RefreshUsers();

        // abre confirmación
        if (menu != null)
            menu.OnStudentPicked();
    }
}