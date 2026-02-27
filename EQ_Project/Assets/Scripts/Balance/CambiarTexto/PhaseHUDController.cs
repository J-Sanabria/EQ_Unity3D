using UnityEngine;
using UnityEngine.UI;

public class PhaseHUDController : MonoBehaviour, IPhaseHUD
{
    [Header("Icons")]
    [SerializeField] Image metalsIcon;
    [SerializeField] Image nonMetalsIcon;
    [SerializeField] Image hydrogenIcon;
    [SerializeField] Image oxygenIcon;

    [Header("Lock overlays")]
    [SerializeField] GameObject metalsLock;
    [SerializeField] GameObject nonMetalsLock;
    [SerializeField] GameObject hydrogenLock;
    [SerializeField] GameObject oxygenLock;

    [Header("Active highlight")]
    [SerializeField] GameObject metalsActive;
    [SerializeField] GameObject nonMetalsActive;
    [SerializeField] GameObject hydrogenActive;
    [SerializeField] GameObject oxygenActive;

    [Header("Check overlays")]
    [SerializeField] GameObject metalsCheck;
    [SerializeField] GameObject nonMetalsCheck;
    [SerializeField] GameObject hydrogenCheck;
    [SerializeField] GameObject oxygenCheck;

    public void SetPhaseState(PhaseKey key, PhaseState state)
    {
        var ui = GetUI(key);

        if (ui.lockGo != null)
            ui.lockGo.SetActive(state == PhaseState.Locked);

        if (ui.checkGo != null)
            ui.checkGo.SetActive(state == PhaseState.Completed);

        if (ui.icon != null)
            ui.icon.gameObject.SetActive(state != PhaseState.NotPresent);

        // Si está completada, el highlight activo no debe mostrarse
        if (state == PhaseState.Completed && ui.activeGo != null)
            ui.activeGo.SetActive(false);
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
        var ui = GetUI(key);
        if (ui.activeGo != null)
            ui.activeGo.SetActive(on);
    }

    (Image icon, GameObject lockGo, GameObject activeGo, GameObject checkGo) GetUI(PhaseKey key) => key switch
    {
        PhaseKey.Metals => (metalsIcon, metalsLock, metalsActive, metalsCheck),
        PhaseKey.NonMetals => (nonMetalsIcon, nonMetalsLock, nonMetalsActive, nonMetalsCheck),
        PhaseKey.Hydrogen => (hydrogenIcon, hydrogenLock, hydrogenActive, hydrogenCheck),
        PhaseKey.Oxygen => (oxygenIcon, oxygenLock, oxygenActive, oxygenCheck),
        _ => (null, null, null, null)
    };
}