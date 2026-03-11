using CB.Balance;
using UnityEngine;

public class MonitorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private LevelController level;
    [SerializeField] private BalanceStation station;
    [SerializeField] private InteractableHighlight highlight;

    public string Prompt => "E o Enter - Iniciar balanceo";

    void Awake()
    {
        if (station == null)
            station = GetComponentInParent<BalanceStation>();

        if (level == null)
            level = Object.FindFirstObjectByType<LevelController>();

        if (highlight == null)
            highlight = GetComponentInChildren<InteractableHighlight>();

        if (station == null)
            Debug.LogError("[MonitorInteractable] Falta BalanceStation", this);

        if (level == null)
            Debug.LogError("[MonitorInteractable] Falta LevelController", this);
    }

    void Reset()
    {
        if (station == null) station = GetComponentInParent<BalanceStation>();
        if (level == null) level = Object.FindFirstObjectByType<LevelController>();
        if (highlight == null) highlight = GetComponentInChildren<InteractableHighlight>();
    }

    public void Interact(Transform interactor)
    {
        if (level == null || station == null)
            return;

        level.RequestStartBalance(station);
    }

    public void SetFocused(bool focused)
    {
        highlight?.SetFocused(focused);
    }
}