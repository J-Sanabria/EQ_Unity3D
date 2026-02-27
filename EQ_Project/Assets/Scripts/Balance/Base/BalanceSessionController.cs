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

        public BalanceStation Station { get; private set; }

        /// <summary>
        /// Hook opcional para restringir ajustes desde otro sistema (fases/llaves/dificultad).
        /// Firma: (side, index, delta) => permitido?
        /// side: 0=izq, 1=der
        /// </summary>
        public Func<int, int, int, bool> CanAdjust;

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

            InitFromReaction();
            ResetMetrics();
            running = true;
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
            if (!running || Station == null || Station.reaction == null)
                return;

            if (delta == 0) return;
            if (side != 0 && side != 1) return;

            // Hook de permisos (fases/llaves/dificultad)
            if (CanAdjust != null && !CanAdjust(side, index, delta))
            {
                Debug.Log($"[Session] Adjust BLOQUEADO side={side} idx={index} delta={delta}");
                return;
            }
            var coefs = side == 0 ? coefL : coefR;
            if (coefs == null || index < 0 || index >= coefs.Length)
                return;

            int before = coefs[index];
            int after = Mathf.Clamp(before + delta, minCoef, maxCoef);

            if (after == before) return;

            coefs[index] = after;
            OnEquationChanged?.Invoke();
        }

        // -------------------------
        // Validación
        // -------------------------
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
    }
}