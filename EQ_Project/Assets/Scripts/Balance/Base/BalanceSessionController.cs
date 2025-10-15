using UnityEngine;
using System.Collections.Generic;
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

    public class BalanceSessionController : MonoBehaviour, IBalanceUIScreen
    {
        [Header("UI")]
        [SerializeField] EquationHUD equationHUD;

        [Header("Estado")]
        public int[] coefL;
        public int[] coefR;

        [Header("Progreso")]
        public int errorCount = 0;
        public float elapsed;                   // tiempo en modo Balance
        bool running;

        [Header("Puntaje")]
        [Tooltip("Puntaje base si acierta")]
        public int baseScore = 1000;
        [Tooltip("Penalización por cada error (fallo al verificar)")]
        public int penaltyPerError = 100;
        [Tooltip("Penalización por segundo")]
        public int penaltyPerSecond = 2;
        [Tooltip("Puntaje mínimo")]
        public int minScore = 0;

        [Header("Inventario")]
        [SerializeField] PlayerInventory inventory;        // tu inventario de slots
        [SerializeField] InventoryDatabase database;      // para mapear símbolo -> item
        Dictionary<ItemDefinition, int> reserved = new Dictionary<ItemDefinition, int>(); // reserva temporal


        public BalanceStation Station { get; private set; }

        public event Action<BalanceResult> OnChallengeCompleted;

        public void BindStation(BalanceStation s)
        {
            Station = s;

            if (inventory == null) inventory = FindObjectOfType<PlayerInventory>();
            if (database == null && inventory != null) database = inventory.database;

            reserved.Clear();

            InitFromStation();
            errorCount = 0;
            elapsed = 0f;
            running = true;
            Render();
        }
        void Update()
        {
            if (running) elapsed += Time.deltaTime;
        }

        void InitFromStation()
        {
            if (Station == null || Station.reaction == null) { coefL = coefR = null; return; }
            var rxn = Station.reaction;
            coefL = (int[])rxn.coefL.Clone();
            coefR = (int[])rxn.coefR.Clone();
        }

        public void Adjust(int side, int index, int delta)
        {
            if (delta == 0) return;
            if (Station == null || Station.reaction == null) return;

            string[] list = side == 0 ? Station.reaction.lhs : Station.reaction.rhs;
            int[] cofs = side == 0 ? coefL : coefR;

            if (list == null || cofs == null) return;
            if (index < 0 || index >= list.Length) return;

            string species = list[index];

            // 1) Parseo estequiométrico por unidad (H2O -> H:2, O:1)
            var perUnit = ChemFormula.Parse(species); // Dictionary<string,int>

            if (delta > 0)
            {
                // 2) Verificar inventario disponible por elemento
                foreach (var kv in perUnit)
                {
                    string elem = kv.Key;
                    int need = kv.Value; // para +1 unidad
                    var def = ItemForElement(elem);
                    if (def == null) { Debug.LogWarning("Sin item para elemento: " + elem); return; }

                    int libres = inventory != null ? inventory.CountOf(def) - ReservedOf(def) : 0;
                    if (libres < need) return; // NO alcanza -> rechaza
                }

                // 3) Reservar y aplicar
                foreach (var kv in perUnit)
                {
                    var def = ItemForElement(kv.Key);
                    AddReserve(def, kv.Value);
                }
                cofs[index] += 1;
                Render();
                return;
            }
            else // delta < 0
            {
                if (cofs[index] <= 0) return;

                // 4) Devolver de la reserva
                foreach (var kv in perUnit)
                {
                    var def = ItemForElement(kv.Key);
                    AddReserve(def, -kv.Value);
                }
                cofs[index] -= 1;
                Render();
                return;
            }
        }


        public bool IsBalancedNow()
        {
            if (Station == null || Station.reaction == null) return false;
            var rxn = Station.reaction;
            return ReactionValidator.IsBalanced(rxn.lhs, rxn.rhs, coefL, coefR);
        }

        public void Render(int selectedSide = -1, int selectedIndex = -1)
        {
            if (Station == null || Station.reaction == null || equationHUD == null) return;

            var rxn = Station.reaction;
            var diff = ReactionValidator.Imbalance(rxn.lhs, rxn.rhs, coefL, coefR);
            var bad = new HashSet<string>();
            foreach (var kv in diff) if (kv.Value != 0) bad.Add(kv.Key);

            equationHUD.SetEquation(rxn.lhs, rxn.rhs, coefL, coefR, selectedSide, selectedIndex, bad);
        }

        public int LeftCount { get { return Station != null && Station.reaction?.lhs != null ? Station.reaction.lhs.Length : 0; } }
        public int RightCount { get { return Station != null && Station.reaction?.rhs != null ? Station.reaction.rhs.Length : 0; } }

        // Llamar cuando Verify es correcto
        public void CompleteChallenge()
        {
            if (Station == null || Station.reaction == null) return;

            running = false;

            int score = baseScore
                        - errorCount * penaltyPerError
                        - Mathf.RoundToInt(elapsed) * penaltyPerSecond;
            score = Mathf.Max(minScore, score);

            // Confirmar: quitar la reserva del inventario real
            if (inventory != null)
            {
                foreach (var kv in reserved)
                {
                    var def = kv.Key;
                    int qty = kv.Value;
                    if (def != null && qty > 0)
                        inventory.Remove(def, qty);
                }
            }
            reserved.Clear();

            var result = new BalanceResult
            {
                reactionId = Station.reaction.reactionId,
                timeSeconds = elapsed,
                errors = errorCount,
                score = score
            };

            OnChallengeCompleted?.Invoke(result);
        }


        // Reintentar: resetea tiempo/errores y vuelve a coeficientes iniciales
        public void RestartChallenge()
        {
            reserved.Clear(); // descarta la reserva
            InitFromStation();
            errorCount = 0;
            elapsed = 0f;
            running = true;
            Render();
        }

        ItemDefinition ItemForElement(string elementSymbol)
        {
            if (database == null || string.IsNullOrEmpty(elementSymbol)) return null;
            // SUPOSICIÓN: el id del ItemDefinition en la DB es el símbolo, p.e. "H", "O".
            // Si usas otro id, crea aquí un map símbolo->id y llama database.FindById(map[símbolo]).
            return database.FindById(elementSymbol);
        }

        int ReservedOf(ItemDefinition def)
        {
            if (def == null) return 0;
            int v; return reserved.TryGetValue(def, out v) ? v : 0;
        }

        void AddReserve(ItemDefinition def, int qty)
        {
            if (def == null || qty == 0) return;
            int v; reserved.TryGetValue(def, out v);
            v += qty;
            if (v <= 0) reserved.Remove(def);
            else reserved[def] = v;
        }
    }
}
