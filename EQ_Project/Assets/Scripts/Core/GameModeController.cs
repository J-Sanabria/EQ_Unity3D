using CB.Balance;
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
        [SerializeField] MonoBehaviour playerMovement;
        [SerializeField] MonoBehaviour interactionSensor;
        [SerializeField] MonoBehaviour playerInputs;

        [Header("Cameras")]
        [SerializeField] GameObject gameplayCameraRig;
        [SerializeField] GameObject balanceCameraRig;

        [Header("UI Panels")]
        [SerializeField] GameObject hudTopEquation;
        [SerializeField] GameObject hudExploration;
        [SerializeField] GameObject hudBalance;
        [SerializeField] EquationHUDBinding equationHUD;

        [Header("Cursor")]
        [SerializeField] bool showCursorInBalance = true;

        [Header("Input")]
        [SerializeField] PlayerInput playerInput;
        [SerializeField] string explorationMap = "Gameplay";
        [SerializeField] string balanceMap = "Balance";

        public GameState State { get; private set; } = GameState.Exploration;
        public BalanceStation CurrentStation { get; private set; }
        void Awake()
        {
            ApplyState();
        }

        public void EnterExploration()
        {
            if (State == GameState.Exploration) return;

            CurrentStation = null;
            State = GameState.Exploration;
            ApplyState();
        }

        public void EnterBalance(BalanceStation station)
        {
            Debug.Log("LevelControllerDioLaOrden");
            if (station == null || State == GameState.Balance)
            {
                Debug.LogWarning("No hay station o ya esta en balanceo");
                return;
            }
          

            CurrentStation = station;
            State = GameState.Balance;

            station.session.BindStation(station);

            station.selection.Configure(
                station.reaction.lhs.Length,
                station.reaction.rhs.Length
            );

            equationHUD.Bind(
                station.session,
                station.selection
            );

            var input = station.GetComponent<BalanceInputController>();
            if (input != null)
            {
                input.VerifyPressed += OnVerifyRequested;
                input.ExitPressed += OnExitRequested;
            }

            ApplyState();
            
        }

        public void ExitBalance()
        {
            if (State != GameState.Balance)
                return;

            if (CurrentStation != null)
            {
                var input = CurrentStation.GetComponent<BalanceInputController>();
                if (input != null)
                {
                    input.VerifyPressed -= OnVerifyRequested;
                    input.ExitPressed -= OnExitRequested;
                }
            }

            EnterExploration();
        }

        void ApplyState()
        {
            // Player
            SetEnabled(playerMovement, State == GameState.Exploration);
            SetEnabled(interactionSensor, State == GameState.Exploration);
            SetEnabled(playerInputs, State == GameState.Exploration);

            // Cameras
            if (gameplayCameraRig)
                gameplayCameraRig.SetActive(State == GameState.Exploration);

            if (balanceCameraRig)
                balanceCameraRig.SetActive(State == GameState.Balance);

            // UI
            if (hudTopEquation)
                hudTopEquation.SetActive(true);

            if (hudExploration)
                hudExploration.SetActive(State == GameState.Exploration);

            if (hudBalance)
                hudBalance.SetActive(State == GameState.Balance);

            // Cursor
            SetCursor(State == GameState.Balance && showCursorInBalance);

            // Input maps
            if (playerInput != null)
            {
                if (State == GameState.Exploration && !string.IsNullOrEmpty(explorationMap))
                    playerInput.SwitchCurrentActionMap(explorationMap);

                if (State == GameState.Balance && !string.IsNullOrEmpty(balanceMap))
                    playerInput.SwitchCurrentActionMap(balanceMap);
            }
        }

        static void SetEnabled(MonoBehaviour mb, bool enabled)
        {
            if (mb != null) mb.enabled = enabled;
        }

        static void SetCursor(bool visible)
        {
            Cursor.visible = visible;
            Cursor.lockState = visible
                ? CursorLockMode.None
                : CursorLockMode.Locked;
        }

        void OnVerifyRequested()
        {
            if (CurrentStation == null) return;

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
            Debug.Log("[Balance] Salida manual del modo balance");
            ExitBalance();
        }
    }
}
