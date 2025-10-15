using UnityEngine;
using CB.Core;      // GameModeController
using CB.Balance;   // BalanceStation

public class EquationHUDBinding : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameModeController gameMode;    // arrástralo
    [SerializeField] EquationHUD hud;                // arrástralo (el script que ya tienes)
    [SerializeField] ReactionAsset explorationReaction; // lo que quieres ver en Exploración

    // cache simple para no re-renderizar sin cambios
    BalanceStation _lastStation;
    ReactionAsset _lastAsset;

    void Reset()
    {
        if (hud == null) hud = GetComponentInChildren<EquationHUD>();
        if (gameMode == null) gameMode = FindObjectOfType<GameModeController>();
    }

    void LateUpdate()
    {
        // 1) decidir qué reacción mostrar
        ReactionAsset assetToShow = null;

        if (gameMode != null && gameMode.State == GameState.Balance && gameMode.ActiveStation != null)
        {
            assetToShow = gameMode.ActiveStation.reaction;
        }
        else
        {
            assetToShow = explorationReaction;
        }

        // 2) si no hay cambios, no hagas nada
        if (assetToShow == _lastAsset && gameMode.ActiveStation == _lastStation) return;

        _lastAsset = assetToShow;
        _lastStation = gameMode != null ? gameMode.ActiveStation : null;

        // 3) renderizar
        if (hud != null && assetToShow != null)
        {
            hud.SetEquation(
                assetToShow.lhs,
                assetToShow.rhs,
                assetToShow.coefL,
                assetToShow.coefR,
                selectedSide: -1,
                selectedIndex: -1
            );
        }
    }
}
