using UnityEngine;
using System;
using System.Collections.Generic;

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
        [Header("Estado")]
        public int[] coefL;
        public int[] coefR;

        [Header("Progreso")]
        public int errorCount;
        public float elapsed;
        bool running;

        [Header("Score")]
        [SerializeField] int baseScore = 1000;
        [SerializeField] int penaltyPerError = 100;
        [SerializeField] int minScore = 0;

        [Header("Inventario")]
        [SerializeField] PlayerInventory inventory;
        [SerializeField] InventoryDatabase database;
        [SerializeField] bool respectSubscripts = true;

        Dictionary<ItemDefinition, int> reserved = new();

        public BalanceStation Station { get; private set; }

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

            if (inventory == null)
                inventory = FindObjectOfType<PlayerInventory>();

            if (database == null && inventory != null)
                database = inventory.database;

            InitFromReaction();
            ResetMetrics();
            running = true;
        }

        void InitFromReaction()
        {
            if (Station == null || Station.reaction == null)
                return;

            coefL = (int[])Station.reaction.coefL.Clone();
            coefR = (int[])Station.reaction.coefR.Clone();

            for (int i = 0; i < coefL.Length; i++)
                coefL[i] = Mathf.Max(1, coefL[i]);

            for (int i = 0; i < coefR.Length; i++)
                coefR[i] = Mathf.Max(1, coefR[i]);

            reserved.Clear();
            OnEquationChanged?.Invoke();
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

            var species = side == 0
                ? Station.reaction.lhs
                : Station.reaction.rhs;

            var coefs = side == 0 ? coefL : coefR;

            if (index < 0 || index >= species.Length)
                return;

            int before = coefs[index];

            if (delta > 0)
                TryIncrease(species[index], coefs, index);
            else if (delta < 0)
                TryDecrease(species[index], coefs, index);

            if (coefs[index] != before)
                OnEquationChanged?.Invoke();
        }

        void TryIncrease(string formula, int[] coefs, int index)
        {
            var perUnit = ChemFormula.Parse(formula);

            foreach (var kv in perUnit)
            {
                var def = ItemForElement(kv.Key);
                int need = respectSubscripts ? kv.Value : 1;

                if (def == null)
                    return;

                int available = inventory.CountOf(def) - ReservedOf(def);
                if (available < need)
                    return;
            }

            foreach (var kv in perUnit)
                AddReserve(ItemForElement(kv.Key), kv.Value);

            coefs[index]++;
        }

        void TryDecrease(string formula, int[] coefs, int index)
        {
            if (coefs[index] <= 1)
                return;

            var perUnit = ChemFormula.Parse(formula);

            foreach (var kv in perUnit)
                AddReserve(ItemForElement(kv.Key), -kv.Value);

            coefs[index]--;
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

            int score = Mathf.Max(
                minScore,
                baseScore - errorCount * penaltyPerError
            );

            CommitInventory();

            var result = new BalanceResult
            {
                reactionId = Station.reaction.reactionId,
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

        // -------------------------
        // Inventario
        // -------------------------
        void CommitInventory()
        {
            foreach (var kv in reserved)
                inventory.Remove(kv.Key, kv.Value);

            reserved.Clear();
        }

        ItemDefinition ItemForElement(string symbol)
        {
            return database != null ? database.FindById(symbol) : null;
        }

        int ReservedOf(ItemDefinition def)
        {
            return reserved.TryGetValue(def, out int v) ? v : 0;
        }

        void AddReserve(ItemDefinition def, int qty)
        {
            if (def == null || qty == 0)
                return;

            reserved.TryGetValue(def, out int v);
            v += qty;

            if (v <= 0) reserved.Remove(def);
            else reserved[def] = v;
        }
    }
}
