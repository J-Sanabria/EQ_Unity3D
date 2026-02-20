using CB.Balance;
using UnityEngine;

public class MonitorInteractable : Interactable
{
    [SerializeField] LevelController level;
    [SerializeField] BalanceStation station;

    public override void Interact(Transform interactor)
    {
        if (level == null || station == null)
        {
            Debug.LogWarning("MonitorInteractable mal configurado");
            return;
        }

        Debug.Log("ElMonitorDiolaOrden");
        level.RequestStartBalance(station);
    }

    public new string Prompt => "E - Iniciar balanceo";
}
