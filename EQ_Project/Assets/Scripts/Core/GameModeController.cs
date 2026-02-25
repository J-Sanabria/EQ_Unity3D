using CB.Balance;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CB.Core
{
    public enum GameState
    {
        Exploration,
        Balance
    }

    public class GameModeController : MonoBehaviour
    {
        [Header("Player refs")]
        [SerializeField] ThirdPersonController playerMovement;
        [SerializeField] InteractionSensor interactionSensor;

        [Header("Cameras")]
        [SerializeField] GameObject gameplayCameraRig;
        [SerializeField] GameObject balanceCameraRig;

        [Header("UI Panels")]
        [SerializeField] GameObject hudTopEquation;
        [SerializeField] GameObject hudExploration;
        [SerializeField] GameObject hudBalance;
        [SerializeField] EquationHUDBinding equationHUD;

        [Header("Cursor")]
        [SerializeField] bool showCursorInBalance = false;

        [Header("Input")]
        [SerializeField] PlayerInput playerInput;
        [SerializeField] string explorationMap = "Gameplay";
        [SerializeField] string balanceMap = "Balance";

        public event System.Action<GameState> OnStateChanged;
        public GameState State { get; private set; } = GameState.Exploration;
        public BalanceStation CurrentStation { get; private set; }

        void Start()
        {
            ApplyState(forceActionMap: true);
        }

        void OnDisable()
        {
            UnhookStationInput(CurrentStation);
        }

        public void EnterExploration()
        {
            if (State == GameState.Exploration) return;

            UnhookStationInput(CurrentStation);
            CurrentStation = null;

            State = GameState.Exploration;
            ApplyState(forceActionMap: false);
            OnStateChanged?.Invoke(State);
        }

        public void EnterBalance(BalanceStation station)
        {
            if (station == null)
            {
                Debug.LogWarning("[GameMode] EnterBalance: station es null");
                return;
            }

            if (State == GameState.Balance && station == CurrentStation)
                return;

            UnhookStationInput(CurrentStation);

            if (station.reaction == null) { Debug.LogError("[GameMode] station.reaction es null"); return; }
            if (station.session == null) { Debug.LogError("[GameMode] station.session es null"); return; }
            if (station.selection == null) { Debug.LogError("[GameMode] station.selection es null"); return; }
            if (equationHUD == null) { Debug.LogError("[GameMode] equationHUD no asignado"); return; }

            CurrentStation = station;
            State = GameState.Balance;

            station.session.BindStation(station);
            station.selection.Configure(station.reaction.lhs.Length, station.reaction.rhs.Length);
            equationHUD.Bind(station.session, station.selection);

            HookStationInput(station);

            ApplyState(forceActionMap: false);
            OnStateChanged?.Invoke(State);
        }

        public void ExitBalance()
        {
            if (State != GameState.Balance) return;
            EnterExploration();
        }

        void ApplyState(bool forceActionMap)
        {
            bool exploring = State == GameState.Exploration;
            bool balancing = State == GameState.Balance;

            // Player
            if (playerMovement != null)
                playerMovement.MovementEnabled = exploring; // evita animación “pegada”

            if (interactionSensor != null)
                interactionSensor.enabled = exploring; // solo se usa en exploración

            // Cameras
            if (gameplayCameraRig) gameplayCameraRig.SetActive(exploring);
            if (balanceCameraRig) balanceCameraRig.SetActive(balancing);

            // UI
            if (hudTopEquation) hudTopEquation.SetActive(true);
            if (hudExploration) hudExploration.SetActive(exploring);
            if (hudBalance) hudBalance.SetActive(balancing);

            if (equationHUD != null)
                equationHUD.SetMode(balancing); // evita rojo en exploración

            // Cursor
            SetCursor(balancing && showCursorInBalance);

            // Input maps
            if (playerInput != null)
            {
                string targetMap = exploring ? explorationMap : balanceMap;
                if (!string.IsNullOrEmpty(targetMap))
                {
                    bool needsSwitch = forceActionMap ||
                                       playerInput.currentActionMap == null ||
                                       playerInput.currentActionMap.name != targetMap;

                    if (needsSwitch)
                        playerInput.SwitchCurrentActionMap(targetMap);

                    // Importante: resetea latch SOLO al volver a exploración
                    if (exploring && interactionSensor != null)
                        interactionSensor.ResetInteractLatch();
                }
            }
        }

        void HookStationInput(BalanceStation station)
        {
            if (station == null) return;
            var input = station.GetComponent<BalanceInputController>();
            if (input == null) return;

            input.VerifyPressed += OnVerifyRequested;
            input.ExitPressed += OnExitRequested;
        }

        void UnhookStationInput(BalanceStation station)
        {
            if (station == null) return;
            var input = station.GetComponent<BalanceInputController>();
            if (input == null) return;

            input.VerifyPressed -= OnVerifyRequested;
            input.ExitPressed -= OnExitRequested;
        }

        static void SetCursor(bool visible)
        {
            Cursor.visible = visible;
            Cursor.lockState = CursorLockMode.None;
        }

        void OnVerifyRequested()
        {
            if (CurrentStation == null || CurrentStation.session == null) return;

            var session = CurrentStation.session;
            if (session.IsBalanced())
            {
                session.CompleteSession();
            }
            else
            {
                session.RegisterError();
                Debug.Log("[Balance] Ecuación incorrecta");
            }
        }

        void OnExitRequested()
        {
            ExitBalance();
        }
    }
}