using UnityEngine;
using System;

namespace CB.Balance
{
    [Serializable]
    public struct BalanceResult
    {
        public string reactionId;
        public float timeSeconds;
        public int errors;
        public int score;
        public int stepsUsed;
        public int idealSteps;
        public int freeExtraSteps;
        public int extraSteps;

        public int baseScore;
        public int penaltyErrors;
        public int penaltySteps;
        public int penaltyTime;

        public float targetTimeSeconds;
        public bool isTutorial;
    }

    public enum VerifyResult
    {
        BalancedMinimal,
        BalancedNotMinimal,
        Incorrect
    }
    public enum AdjustBlockReason
    {
        None,
        NoKeys,
        PhaseLocked,
        WrongPhaseOrder
    }

    public class BalanceSessionController : MonoBehaviour
    {
        [Header("Estado (coeficientes actuales)")]
        public int[] coefL;
        public int[] coefR;

        [Header("Progreso")]
        public int errorCount;
        public float elapsed;
        [SerializeField] private bool running;
        [SerializeField] private bool hasStartedOnce;
        public bool HasStartedOnce => hasStartedOnce;
        public bool IsRunning => running;



        [Header("Coeficientes")]
        [SerializeField] private int minCoef = 1;
        [SerializeField] private int maxCoef = 12;

        [Header("Score")]
        [SerializeField] private int baseScore = 1000;
        [SerializeField] private int penaltyPerError = 100;
        [SerializeField] private int minScore = 0;

        [Header("Advanced Score")]
        [SerializeField] private int penaltyPerExtraStep = 10;
        [SerializeField] private int penaltyPerTimeBlock = 10;
        [SerializeField] private float timePenaltyBlockSeconds = 10f;

        [Header("Free Extra Steps By Difficulty")]
        [SerializeField] private int easyFreeExtraSteps = 5;
        [SerializeField] private int mediumFreeExtraSteps = 3;
        [SerializeField] private int hardFreeExtraSteps = 1;

        private Difficulty currentDifficulty = Difficulty.Tutorial;

        string _boundReactionId;
        public BalanceStation Station { get; private set; }

        public event Action<VerifyResult> OnVerifyFeedback;
        public event Action<int, int, int, int> OnAdjustedApplied;
        public Func<int, int, int, AdjustBlockReason> CanAdjustReason;
        public event Action<AdjustBlockReason> OnAdjustBlocked;

        public event Action<BalanceResult> OnSessionCompleted;
        public event Action OnEquationChanged;
        public int adjustmentCount;

        void Update()
        {
            if (running)
                elapsed += Time.deltaTime;
        }

        // -------------------------
        // Inicialización
        // -------------------------
        public void BindStation(BalanceStation station)
        {
            Station = station;

            string rid = Station != null && Station.reaction != null ? Station.reaction.reactionId : null;

            bool sameReaction =
                !string.IsNullOrEmpty(rid) &&
                rid == _boundReactionId &&
                coefL != null && coefR != null &&
                Station.reaction != null &&
                coefL.Length == Station.reaction.lhs.Length &&
                coefR.Length == Station.reaction.rhs.Length;

            if (!sameReaction)
            {
                _boundReactionId = rid;
                InitFromReaction();
                ResetMetrics();
                running = false;
                Debug.Log("[BalanceSessionController] BindStation -> nueva reacción, timer en false");
            }
            else
            {
                running = false;
                OnEquationChanged?.Invoke();
                Debug.Log("[BalanceSessionController] BindStation -> misma reacción, timer en false");
            }
        }

        public void StartSessionTimer()
        {
            hasStartedOnce = true;
            running = true;
        }

        public void PauseSessionTimer()
        {
            running = false;
            Debug.Log("[BalanceSessionController] Timer PAUSE");
        }

        public void SetDifficulty(Difficulty difficulty)
        {
            currentDifficulty = difficulty;
        }
        void InitFromReaction()
        {
            if (Station == null || Station.reaction == null)
            {
                coefL = Array.Empty<int>();
                coefR = Array.Empty<int>();
                OnEquationChanged?.Invoke();
                return;
            }

            coefL = (int[])Station.reaction.coefL.Clone();
            coefR = (int[])Station.reaction.coefR.Clone();

            ClampAllCoefs();
            OnEquationChanged?.Invoke();
        }

        void ClampAllCoefs()
        {
            if (coefL != null)
                for (int i = 0; i < coefL.Length; i++)
                    coefL[i] = Mathf.Clamp(coefL[i], minCoef, maxCoef);

            if (coefR != null)
                for (int i = 0; i < coefR.Length; i++)
                    coefR[i] = Mathf.Clamp(coefR[i], minCoef, maxCoef);
        }

        void ResetMetrics()
        {
            errorCount = 0;
            elapsed = 0f;
            adjustmentCount = 0;
            hasStartedOnce = false;
        }

        // -------------------------
        // Interacción
        // -------------------------
        public void Adjust(int side, int index, int delta)
        {
            if (!running || Station == null || Station.reaction == null) return;
            if (delta == 0) return;
            if (side != 0 && side != 1) return;

            if (CanAdjustReason != null)
            {
                var reason = CanAdjustReason(side, index, delta);
                if (reason != AdjustBlockReason.None)
                {
                    OnAdjustBlocked?.Invoke(reason);
                    return;
                }
            }

            var coefs = side == 0 ? coefL : coefR;
            if (coefs == null || index < 0 || index >= coefs.Length) return;

            int before = coefs[index];
            int after = Mathf.Clamp(before + delta, minCoef, maxCoef);
            if (after == before) return;

            coefs[index] = after;
            adjustmentCount++;
            OnEquationChanged?.Invoke();
            OnAdjustedApplied?.Invoke(side, index, before, after);
        }

        // -------------------------
        // Validación
        // -------------------------

        public void Verify()
        {
            if (!running || Station == null || Station.reaction == null)
                return;

            if (IsBalanced())
            {
                if (IsBalancedMinimal())
                {
                    OnVerifyFeedback?.Invoke(VerifyResult.BalancedMinimal);
                    CompleteSession();
                }
                else
                {
                    RegisterError();
                    OnVerifyFeedback?.Invoke(VerifyResult.BalancedNotMinimal);
                }
            }
            else
            {
                RegisterError();
                OnVerifyFeedback?.Invoke(VerifyResult.Incorrect);
            }
        }
        public bool IsBalanced()
        {
            if (Station == null || Station.reaction == null)
                return false;

            return ReactionValidator.IsBalanced(
                Station.reaction.lhs,
                Station.reaction.rhs,
                coefL,
                coefR
            );
        }

        public bool IsBalancedMinimal()
        {
            if (!IsBalanced()) return false;

            int g = 0;
            for (int i = 0; i < coefL.Length; i++) g = Gcd(g, coefL[i]);
            for (int i = 0; i < coefR.Length; i++) g = Gcd(g, coefR[i]);

            return g == 1;
        }

        static int Gcd(int a, int b)
        {
            a = Mathf.Abs(a);
            b = Mathf.Abs(b);
            if (a == 0) return b;
            while (b != 0)
            {
                int t = a % b;
                a = b;
                b = t;
            }
            return a;
        }

        public void RegisterError()
        {
            errorCount++;
        }

        // -------------------------
        // Finalización
        // -------------------------

        private int GetFreeExtraSteps()
        {
            switch (currentDifficulty)
            {
                case Difficulty.Easy: return easyFreeExtraSteps;
                case Difficulty.Medium: return mediumFreeExtraSteps;
                case Difficulty.Hard: return hardFreeExtraSteps;
                case Difficulty.Tutorial:
                default: return 0;
            }
        }

        private float GetTargetTimeSeconds()
        {
            if (Station.reaction == null)
                return 0f;

            switch (currentDifficulty)
            {
                case Difficulty.Easy: return Station.reaction.easyTargetTime;
                case Difficulty.Medium: return Station.reaction.mediumTargetTime;
                case Difficulty.Hard: return Station.reaction.hardTargetTime;
                case Difficulty.Tutorial:
                default: return 0f;
            }
        }

        private int CalculateTimePenalty(float elapsedSeconds, float targetSeconds)
        {
            if (currentDifficulty == Difficulty.Tutorial)
                return 0;

            if (targetSeconds <= 0f || elapsedSeconds <= targetSeconds)
                return 0;

            float extraTime = elapsedSeconds - targetSeconds;
            int blocks = Mathf.FloorToInt(extraTime / timePenaltyBlockSeconds);
            return blocks * penaltyPerTimeBlock;
        }
        private void CompleteSession()
        {
            bool isTutorial = currentDifficulty == Difficulty.Tutorial;

            int finalScore;
            int usedSteps = adjustmentCount;
            int idealSteps = Station.reaction != null ? Station.reaction.idealSteps : 0;
            int freeExtraSteps = isTutorial ? 0 : GetFreeExtraSteps();
            int extraSteps = isTutorial ? 0 : Mathf.Max(0, usedSteps - idealSteps - freeExtraSteps);

            int errorPenalty = isTutorial ? 0 : errorCount * penaltyPerError;
            int stepPenalty = isTutorial ? 0 : extraSteps * penaltyPerExtraStep;

            float targetTime = isTutorial
                ? 0f
                : GetTargetTimeSeconds();

            int timePenalty = isTutorial
                ? 0
                : CalculateTimePenalty(elapsed, targetTime);

            if (isTutorial)
            {
                finalScore = Station.reaction != null ? Station.reaction.tutorialFixedScore : baseScore;
            }
            else
            {
                finalScore = baseScore - errorPenalty - stepPenalty - timePenalty;
                finalScore = Mathf.Max(minScore, finalScore);
            }

            BalanceResult result = new BalanceResult
            {
                reactionId = Station.reaction != null ? Station.reaction.reactionId : "",
                timeSeconds = elapsed,
                errors = errorCount,
                score = finalScore,

                stepsUsed = usedSteps,
                idealSteps = idealSteps,
                freeExtraSteps = freeExtraSteps,
                extraSteps = extraSteps,

                baseScore = isTutorial && Station.reaction != null ? Station.reaction.tutorialFixedScore : baseScore,
                penaltyErrors = errorPenalty,
                penaltySteps = stepPenalty,
                penaltyTime = timePenalty,

                targetTimeSeconds = targetTime,
                isTutorial = isTutorial
            };

            running = false;
            OnSessionCompleted?.Invoke(result);
        }

        public void Restart()
        {
            InitFromReaction();
            ResetMetrics();
            running = false;
            OnEquationChanged?.Invoke();
        }

        public void Stop()
        {
            running = false;
        }

        // Reset

        public void ResetCoefsToOnes()
        {
            if (Station == null || Station.reaction == null)
                return;

            int l = Station.reaction.lhs.Length;
            int r = Station.reaction.rhs.Length;

            if (coefL == null || coefL.Length != l) coefL = new int[l];
            if (coefR == null || coefR.Length != r) coefR = new int[r];

            for (int i = 0; i < l; i++) coefL[i] = minCoef; // minCoef = 1
            for (int i = 0; i < r; i++) coefR[i] = minCoef;

            ResetMetrics();
            running = false;

            // IMPORTANT: marca como “misma reacción” para que al re-entrar no vuelva a clonar defaults raros
            _boundReactionId = Station.reaction.reactionId;

            OnEquationChanged?.Invoke();
        }
    }
}