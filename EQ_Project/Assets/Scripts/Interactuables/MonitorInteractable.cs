using CB.Balance;
using UnityEngine;

public class MonitorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private LevelController level;
    [SerializeField] private BalanceStation station;

    public string Prompt => "Interactuar - Iniciar balanceo";

    void Reset()
    {
        if (station == null) station = GetComponentInParent<BalanceStation>();
        if (level == null) level = Object.FindFirstObjectByType<LevelController>();
    }

    public void Interact(Transform interactor)
    {
        if (level == null) level = Object.FindFirstObjectByType<LevelController>();
        if (station == null) station = GetComponentInParent<BalanceStation>();

        if (level == null || station == null)
        {
            Debug.LogWarning("[MonitorInteractable] Falta LevelController o BalanceStation");
            return;
        }

        level.RequestStartBalance(station);
    }

    public void SetFocused(bool focused) { }
}