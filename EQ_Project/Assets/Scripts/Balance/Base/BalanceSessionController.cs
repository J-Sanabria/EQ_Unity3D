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

        [Header("Coeficientes")]
        [SerializeField] private int minCoef = 1;
        [SerializeField] private int maxCoef = 12;

        [Header("Score")]
        [SerializeField] private int baseScore = 1000;
        [SerializeField] private int penaltyPerError = 100;
        [SerializeField] private int minScore = 0;

        string _boundReactionId;
        public BalanceStation Station { get; private set; }

        public event Action<VerifyResult> OnVerifyFeedback;
        public event Action<int, int, int, int> OnAdjustedApplied;
        public Func<int, int, int, AdjustBlockReason> CanAdjustReason;
        public event Action<AdjustBlockReason> OnAdjustBlocked;

        public event Action<BalanceResult> OnSessionCompleted;
        public event Action OnEquationChanged;

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
                ResetMetrics(); // opcional: si NO quieres resetear tiempo/errores al re-entrar, solo hazlo aquí
                running = true;
            }
            else
            {
                // misma reacción: NO toques coeficientes
                running = true;
                OnEquationChanged?.Invoke(); // refresca HUD al entrar
            }
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
        public void CompleteSession()
        {
            if (!running)
                return;

            running = false;

            int score = Mathf.Max(minScore, baseScore - errorCount * penaltyPerError);

            var result = new BalanceResult
            {
                reactionId = Station != null && Station.reaction != null ? Station.reaction.reactionId : "",
                timeSeconds = elapsed,
                errors = errorCount,
                score = score
            };

            OnSessionCompleted?.Invoke(result);
        }

        public void Restart()
        {
            InitFromReaction();
            ResetMetrics();
            running = true;
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
            running = true;

            // IMPORTANT: marca como “misma reacción” para que al re-entrar no vuelva a clonar defaults raros
            _boundReactionId = Station.reaction.reactionId;

            OnEquationChanged?.Invoke();
        }
    }
}