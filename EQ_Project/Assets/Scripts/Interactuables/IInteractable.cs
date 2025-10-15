using UnityEngine;

public interface IInteractable
{
    string Prompt { get; }                    // Texto para el HUD, por ejemplo: "E - Usar"
    void SetFocused(bool focused);            // Llamado cuando el jugador enfoca o deja de enfocar
    void Interact(Transform interactor);      // Acción al presionar Interact
}