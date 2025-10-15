using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] string prompt = "E - Interactuar";
    [SerializeField] GameObject highlight;   // opcional, por ejemplo un outline u objeto hijo

    public string Prompt => prompt;

    public virtual void SetFocused(bool focused)
    {
        if (highlight) highlight.SetActive(focused);
    }

    public virtual void Interact(Transform interactor)
    {
        Debug.Log($"Interact: {name} por {interactor.name}");
        // Sobrescribe en clases hijas para comportamiento real
    }
}
