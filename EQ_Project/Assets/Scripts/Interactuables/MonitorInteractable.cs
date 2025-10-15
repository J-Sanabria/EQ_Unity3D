using UnityEngine;
using CB.Core;

namespace CB.Balance
{
    [RequireComponent(typeof(Collider))]
    public class MonitorInteractable : Interactable
    {
        [Header("Refs")]
        public GameModeController gameMode;
        public BalanceStation station;

        public override void Interact(Transform interactor)
        {
            if (gameMode == null || station == null) return;
            if (gameMode.State == GameState.Balance) return;

            gameMode.EnterBalance(station);
        }

        // Prompt opcional distinto
        public new string Prompt => "E - Iniciar balanceo";
    }
}
