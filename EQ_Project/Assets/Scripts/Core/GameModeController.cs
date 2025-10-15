using UnityEngine;
using CB.Balance;

namespace CB.Core
{
    public enum GameState { Exploration, Balance, Pause }

    public class GameModeController : MonoBehaviour
    {
        [Header("Player refs")]
        [SerializeField] MonoBehaviour playerMovement;      // ej. ThirdPersonController
        [SerializeField] MonoBehaviour interactionSensor;   // tu InteractionSensor
        [SerializeField] MonoBehaviour playerInputs;        // StarterAssetsInputs (para bloquear input si hace falta)

        [Header("Cameras (activa solo una)")]
        [SerializeField] GameObject gameplayCameraRig;      // vcam principal o rig gameplay
        [SerializeField] GameObject balanceCameraRig;       // vcam enfocada al monitor

        [Header("UI Panels")]
        [SerializeField] GameObject hudTopEquation;         // siempre visible (modo compacto si quieres)
        [SerializeField] GameObject hudExploration;         // prompts, hotbar, etc.
        [SerializeField] GameObject hudBalance;             // panel grande de balanceo
        [SerializeField] GameObject hudPause;               // panel de pausa

        [Header("Cursor")]
        [SerializeField] bool showCursorInBalance = true;
        [SerializeField] bool showCursorInPause = true;

        public GameState State { get; private set; } = GameState.Exploration;

        // Estacion de balance activa (por si tienes varias)
        public BalanceStation ActiveStation { get; private set; }

        bool _transitionLock;

        void Awake()
        {
            ApplyState();
        }

        void Update()
        {
            // Tecla ESC: salir de Balance o abrir/cerrar Pausa
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (State == GameState.Balance) ExitBalance();
                else if (State == GameState.Exploration) EnterPause();
                else if (State == GameState.Pause) ExitPause();
            }
        }

        public void EnterBalance(BalanceStation station)
        {
            if (_transitionLock || station == null) return;
            _transitionLock = true;

            ActiveStation = station;
            State = GameState.Balance;
            ApplyState();

            _transitionLock = false;
        }

        public void ExitBalance()
        {
            if (_transitionLock) return;
            _transitionLock = true;

            State = GameState.Exploration;
            ActiveStation = null;
            ApplyState();

            _transitionLock = false;
        }

        public void EnterPause()
        {
            if (_transitionLock) return;
            _transitionLock = true;

            State = GameState.Pause;
            ApplyState();

            _transitionLock = false;
        }

        public void ExitPause()
        {
            if (_transitionLock) return;
            _transitionLock = true;

            State = (ActiveStation != null) ? GameState.Balance : GameState.Exploration;
            ApplyState();

            _transitionLock = false;
        }

        void ApplyState()
        {
            // Player movement y sensor
            SetEnabled(playerMovement, State == GameState.Exploration);
            SetEnabled(interactionSensor, State == GameState.Exploration);

            // Si necesitas “anular” por completo entradas mientras no exploras:
            SetEnabled(playerInputs, State == GameState.Exploration);

            // Cámaras
            if (gameplayCameraRig) gameplayCameraRig.SetActive(State != GameState.Balance);
            if (balanceCameraRig) balanceCameraRig.SetActive(State == GameState.Balance);

            // UI
            if (hudTopEquation) hudTopEquation.SetActive(true); // siempre visible; adentro puedes cambiar modo
            if (hudExploration) hudExploration.SetActive(State == GameState.Exploration);
            if (hudBalance) hudBalance.SetActive(State == GameState.Balance);
            if (hudPause) hudPause.SetActive(State == GameState.Pause);

            // Cursor
            switch (State)
            {
                case GameState.Exploration:
                    SetCursor(false);
                    break;
                case GameState.Balance:
                    SetCursor(showCursorInBalance);
                    break;
                case GameState.Pause:
                    SetCursor(showCursorInPause);
                    break;
            }

            // Aviso a la UI de balance (si existe) de la estación activa
            if (State == GameState.Balance && hudBalance != null)
            {
                var ui = hudBalance.GetComponentInChildren<IBalanceUIScreen>();
                if (ui != null) ui.BindStation(ActiveStation);
            }
        }

        static void SetEnabled(MonoBehaviour mb, bool on)
        {
            if (mb != null) mb.enabled = on;
        }

        static void SetCursor(bool visible)
        {
            Cursor.visible = visible;
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
