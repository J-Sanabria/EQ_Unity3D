using UnityEngine;
using UnityEngine.UI;

public class PhaseHUDController : MonoBehaviour, IPhaseHUD
{
    [Header("Icons")]
    [SerializeField] Image metalsIcon;
    [SerializeField] Image nonMetalsIcon;
    [SerializeField] Image hydrogenIcon;
    [SerializeField] Image oxygenIcon;

    [Header("Lock overlays (optional)")]
    [SerializeField] GameObject metalsLock;
    [SerializeField] GameObject nonMetalsLock;
    [SerializeField] GameObject hydrogenLock;
    [SerializeField] GameObject oxygenLock;

    [Header("Active highlight (optional)")]
    [SerializeField] GameObject metalsActive;
    [SerializeField] GameObject nonMetalsActive;
    [SerializeField] GameObject hydrogenActive;
    [SerializeField] GameObject oxygenActive;

    public void SetPhaseState(PhaseKey key, PhaseState state)
    {
        var (lockGo, activeGo) = GetUI(key);

        if (lockGo != null)
            lockGo.SetActive(state == PhaseState.Locked);

        // Si la fase no está presente, puedes ocultar el icono o dejarlo gris
        // Aquí uso ocultar el icono:
        var icon = GetIcon(key);
        if (icon != null)
            icon.gameObject.SetActive(state != PhaseState.NotPresent);

        // Si está completada, puedes cambiar sprite/color desde aquí si quieres
    }

    public void SetActivePhase(PhaseKey? key)
    {
        SetActive(PhaseKey.Metals, key == PhaseKey.Metals);
        SetActive(PhaseKey.NonMetals, key == PhaseKey.NonMetals);
        SetActive(PhaseKey.Hydrogen, key == PhaseKey.Hydrogen);
        SetActive(PhaseKey.Oxygen, key == PhaseKey.Oxygen);
    }

    void SetActive(PhaseKey key, bool on)
    {
        var (_, activeGo) = GetUI(key);
        if (activeGo != null) activeGo.SetActive(on);
    }

    Image GetIcon(PhaseKey key) => key switch
    {
        PhaseKey.Metals => metalsIcon,
        PhaseKey.NonMetals => nonMetalsIcon,
        PhaseKey.Hydrogen => hydrogenIcon,
        PhaseKey.Oxygen => oxygenIcon,
        _ => null
    };

    (GameObject lockGo, GameObject activeGo) GetUI(PhaseKey key) => key switch
    {
        PhaseKey.Metals => (metalsLock, metalsActive),
        PhaseKey.NonMetals => (nonMetalsLock, nonMetalsActive),
        PhaseKey.Hydrogen => (hydrogenLock, hydrogenActive),
        PhaseKey.Oxygen => (oxygenLock, oxygenActive),
        _ => (null, null)
    };
}