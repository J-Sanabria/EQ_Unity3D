using UnityEngine;

public interface IInteractable
{
    string Prompt { get; }
    void Interact(Transform interactor);
    void SetFocused(bool focused);
}