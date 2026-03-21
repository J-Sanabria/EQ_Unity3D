using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIMenuSelectionDriver : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference navigateAction;
    [SerializeField] private InputActionReference pointAction;

    private GameObject _currentFirstSelected;

    private void OnEnable()
    {
        if (navigateAction != null && navigateAction.action != null)
            navigateAction.action.performed += OnNavigatePerformed;

        if (pointAction != null && pointAction.action != null)
            pointAction.action.performed += OnPointPerformed;
    }

    private void OnDisable()
    {
        if (navigateAction != null && navigateAction.action != null)
            navigateAction.action.performed -= OnNavigatePerformed;

        if (pointAction != null && pointAction.action != null)
            pointAction.action.performed -= OnPointPerformed;
    }

    public void SetFirstSelected(GameObject go, bool clearCurrentSelection = true)
    {
        _currentFirstSelected = go;

        if (clearCurrentSelection && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnNavigatePerformed(InputAction.CallbackContext ctx)
    {
        if (EventSystem.current == null) return;
        if (_currentFirstSelected == null) return;

        Vector2 nav = ctx.ReadValue<Vector2>();
        if (nav.sqrMagnitude <= 0.01f) return;

        // Solo selecciona si no hay nada actualmente seleccionado
        if (EventSystem.current.currentSelectedGameObject == null)
            EventSystem.current.SetSelectedGameObject(_currentFirstSelected);
    }

    private void OnPointPerformed(InputAction.CallbackContext ctx)
    {
        if (EventSystem.current == null) return;

        // Al volver al mouse, limpia la selección por teclado
        if (EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}