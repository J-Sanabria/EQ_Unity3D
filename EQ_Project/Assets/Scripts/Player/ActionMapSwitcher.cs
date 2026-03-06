using UnityEngine;
using UnityEngine.InputSystem;

public class ActionMapSwitcher : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] string defaultGameplayMap = "Player";
    [SerializeField] string uiMap = "UI";

    public bool IsUIActive => _depth > 0;
    public string CurrentMapName => playerInput != null && playerInput.currentActionMap != null ? playerInput.currentActionMap.name : "";

    string _prevMap;
    int _depth;

    void Reset()
    {
        if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInput>();
    }

    public void PushUI()
    {
        if (playerInput == null) return;

        if (_depth == 0)
            _prevMap = playerInput.currentActionMap != null ? playerInput.currentActionMap.name : "";

        _depth++;

        if (!string.IsNullOrEmpty(uiMap))
            playerInput.SwitchCurrentActionMap(uiMap);
    }

    public void Pop()
    {
        if (playerInput == null) return;
        if (_depth <= 0) return;

        _depth--;

        if (_depth > 0)
        {
            // alguien más sigue necesitando UI
            if (!string.IsNullOrEmpty(uiMap))
                playerInput.SwitchCurrentActionMap(uiMap);
            return;
        }

        if (!string.IsNullOrEmpty(_prevMap))
            playerInput.SwitchCurrentActionMap(_prevMap);
        else
            playerInput.SwitchCurrentActionMap(defaultGameplayMap);

        _prevMap = "";
    }

    public void ForceToGameplay()
    {
        _depth = 0;
        _prevMap = "";
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap(defaultGameplayMap);
    }
}