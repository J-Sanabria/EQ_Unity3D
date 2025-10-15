using UnityEngine;

public class AdjustSpawnerAmountInteractable : Interactable
{
    [Header("Target")]
    public SimpleSpawnerInteractable target;

    [Header("Ajuste")]
    public int delta = +1; // pon -1 para el boton de disminuir

    public override void Interact(Transform interactor)
    {
        if (target == null) return;
        target.AdjustAmount(delta);
    }

    // (Opcional) prompt visible distinto para 
    public new string Prompt
    {
        get
        {
            return delta >= 0 ? "E - Aumentar cantidad" : "E - Disminuir cantidad";
        }
    }
}
