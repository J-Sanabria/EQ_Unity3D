using System;
using System.Collections.Generic;
using UnityEngine;
using CB.Balance;

public class PhaseManager : MonoBehaviour, IKeyReceiver
{
    [Header("Refs")]
    [SerializeField] private BalanceStation station;
    [SerializeField] private BalanceSessionController session;

    [Header("HUDs (Exploration + Balance)")]
    [SerializeField] private MonoBehaviour[] hudBehaviours;

    [Header("Rules")]
    [SerializeField] private Difficulty difficulty = Difficulty.Tutorial;
    [SerializeField] private bool enforceOrderInTutorialAndEasy = true;

    [Header("Element DB")]
    [SerializeField] private ElementDatabase elementDatabase;

    private readonly List<IPhaseHUD> _huds = new();

    private readonly Dictionary<PhaseKey, PhaseState> _states = new();
    private readonly HashSet<PhaseKey> _present = new();
    private PhaseKey? _activePhase = null;

    void Awake()
    {
        _huds.Clear();
        if (hudBehaviours != null)
        {
            foreach (var mb in hudBehaviours)
            {
                if (mb == null) continue;
                if (mb is IPhaseHUD h) _huds.Add(h);
                else Debug.LogError($"[PhaseManager] {mb.name} no implementa IPhaseHUD");
            }
        }
    }

    void OnEnable()
    {
        if (session != null) session.CanAdjust = CanAdjust;
    }

    void OnDisable()
    {
        if (session != null && session.CanAdjust == CanAdjust) session.CanAdjust = null;
        if (session != null)
            session.OnEquationChanged -= OnEquationChanged;
    }

    public void ConfigureForReaction(BalanceStation st, Difficulty diff)
    {
        if (session != null)
            session.OnEquationChanged -= OnEquationChanged;

        station = st;
        difficulty = diff;
        session = st != null ? st.session : null;

        if (session != null)
        {
            session.CanAdjust = CanAdjust;
            session.OnEquationChanged += OnEquationChanged;
        }

        BuildPhasePresence();
        InitStates();
        SelectInitialActivePhase();
        EvaluatePhaseCompletion();
        PushStateToHUDs();
    }

    // ---------- HUD fan-out ----------
    void PushStateToHUDs()
    {
        foreach (var hud in _huds)
        {
            foreach (var kv in _states)
                hud.SetPhaseState(kv.Key, kv.Value);

            hud.SetActivePhase(_activePhase);
        }
    }

    // ---------- Keys ----------
    public bool ReceiveKey(PhaseKey key, Transform source)
    {
        if (!_present.Contains(key)) return false;
        if (_states[key] == PhaseState.Unlocked || _states[key] == PhaseState.Completed) return false;

        _states[key] = PhaseState.Unlocked;

        _states[key] = PhaseState.Unlocked;
        EvaluatePhaseCompletion(); // recalcula checks + fase activa con el estado real
        PushStateToHUDs();
        return true;
    }

    bool ShouldEnforceOrder()
    {
        return enforceOrderInTutorialAndEasy && (difficulty == Difficulty.Tutorial || difficulty == Difficulty.Easy);
    }

    public HashSet<PhaseKey> GetPresentPhases()
    {
        return new HashSet<PhaseKey>(_present);
    }

    // ---------- Permissions ----------
    bool CanAdjust(int side, int index, int delta)
    {
        if (station == null || station.reaction == null) return false;

        var species = side == 0 ? station.reaction.lhs : station.reaction.rhs;
        if (index < 0 || index >= species.Length) return false;

        string formula = species[index];

        // fases presentes en este compuesto (por elementos)
        var phasesInFormula = GetPhasesForFormula(formula);

        // Si la fórmula no tiene fases detectables (raro), no bloquees.
        if (phasesInFormula.Count == 0) return true;

        Debug.Log($"ActivePhase={_activePhase} | Formula={formula} | PhasesInFormula={string.Join(",", phasesInFormula)} | HState={_states.GetValueOrDefault(PhaseKey.Hydrogen)} | OState={_states.GetValueOrDefault(PhaseKey.Oxygen)}");

        // Regla tutorial/fácil: solo se puede editar si el compuesto contiene la fase activa
        if (ShouldEnforceOrder() && _activePhase.HasValue)
        {
            var ap = _activePhase.Value;

            // si el compuesto no contiene la fase activa, bloquea
            if (!phasesInFormula.Contains(ap))
                return false;

            // si la fase activa está bloqueada, bloquea
            return _states.TryGetValue(ap, out var st) && st != PhaseState.Locked;
        }

        // Regla libre (medio/difícil): no permitir editar compuestos que contengan alguna fase bloqueada
        foreach (var p in phasesInFormula)
        {
            if (_states.TryGetValue(p, out var st) && st == PhaseState.Locked)
                return false;
        }

        return true;
    }

    HashSet<PhaseKey> GetPhasesForFormula(string formula)
    {
        var set = new HashSet<PhaseKey>();

        if (string.IsNullOrEmpty(formula))
            return set;

        var atoms = ChemFormula.Parse(formula);

        bool hasMetal = false;
        bool hasNonMetalOther = false;
        bool hasH = atoms.ContainsKey("H");
        bool hasO = atoms.ContainsKey("O");

        foreach (var e in atoms.Keys)
        {
            if (e == "H" || e == "O") continue;

            if (IsMetal(e)) hasMetal = true;
            else hasNonMetalOther = true;
        }

        if (hasMetal) set.Add(PhaseKey.Metals);
        if (hasNonMetalOther) set.Add(PhaseKey.NonMetals);
        if (hasH) set.Add(PhaseKey.Hydrogen);
        if (hasO) set.Add(PhaseKey.Oxygen);

        return set;
    }

    // ---------- Presence ----------
    void BuildPhasePresence()
    {
        _present.Clear();
        if (station == null || station.reaction == null) return;

        bool hasMetal = false;
        bool hasNonMetalOther = false;
        bool hasH = false;
        bool hasO = false;

        void ScanFormula(string f)
        {
            var atoms = ChemFormula.Parse(f);
            foreach (var e in atoms.Keys)
            {
                if (e == "H") { hasH = true; continue; }
                if (e == "O") { hasO = true; continue; }

                if (IsMetal(e)) hasMetal = true;
                else hasNonMetalOther = true;
            }
        }

        foreach (var f in station.reaction.lhs) ScanFormula(f);
        foreach (var f in station.reaction.rhs) ScanFormula(f);

        if (hasMetal) _present.Add(PhaseKey.Metals);
        if (hasNonMetalOther) _present.Add(PhaseKey.NonMetals);
        if (hasH) _present.Add(PhaseKey.Hydrogen);
        if (hasO) _present.Add(PhaseKey.Oxygen);
    }

    void InitStates()
    {
        _states.Clear();
        foreach (PhaseKey k in Enum.GetValues(typeof(PhaseKey)))
        {
            _states[k] = _present.Contains(k) ? PhaseState.Locked : PhaseState.NotPresent;
        }
    }

    void SelectInitialActivePhase()
    {
        _activePhase = null;
        if (!ShouldEnforceOrder()) return;

        PhaseKey[] order = { PhaseKey.Metals, PhaseKey.NonMetals, PhaseKey.Hydrogen, PhaseKey.Oxygen };
        foreach (var k in order)
        {
            if (_states[k] == PhaseState.Locked || _states[k] == PhaseState.Unlocked)
            {
                _activePhase = k;
                break;
            }
        }
    }


    bool IsMetal(string symbol)
    {
        if (elementDatabase == null) return false;
        return elementDatabase.GetTypeOrDefault(symbol) == ElementType.Metal;
    }

    void OnEquationChanged()
    {
        EvaluatePhaseCompletion();
    }

    void EvaluatePhaseCompletion()
    {
        if (station == null || station.reaction == null || session == null)
            return;

        var imbalance = ReactionValidator.Imbalance(
            station.reaction.lhs,
            station.reaction.rhs,
            session.coefL,
            session.coefR
        );

        // Marca completadas las fases cuyo conjunto de elementos esté equilibrado
        var keys = new List<PhaseKey>(_states.Keys);
        foreach (var k in keys)
        {
            if (_states[k] == PhaseState.NotPresent) continue;
            if (_states[k] == PhaseState.Locked) continue;

            if (IsPhaseBalanced(k, imbalance))
                _states[k] = PhaseState.Completed;
            else if (_states[k] == PhaseState.Completed)
                _states[k] = PhaseState.Unlocked;
        }

        // En tutorial/fácil, si la fase activa quedó completada, avanza a la siguiente
        if (ShouldEnforceOrder())
            _activePhase = ComputeActivePhase(imbalance);
        else
            _activePhase = null; // en medio/difícil puedes no forzar fase activa

        PushStateToHUDs();
    }

    PhaseKey? ComputeActivePhase(Dictionary<string, int> imbalance)
    {
        PhaseKey[] order = { PhaseKey.Metals, PhaseKey.NonMetals, PhaseKey.Hydrogen, PhaseKey.Oxygen };

        foreach (var k in order)
        {
            if (!_states.TryGetValue(k, out var st)) continue;
            if (st == PhaseState.NotPresent) continue;

            bool phaseBalanced = IsPhaseBalanced(k, imbalance);

            // Si está desbalanceada y está locked => sigue siendo la fase activa,
            // pero el jugador verá candado y entenderá que necesita llave.
            if (!phaseBalanced && st == PhaseState.Locked)
                return k;

            // Si está desbalanceada y no está locked => es la fase a trabajar ahora
            if (!phaseBalanced && st != PhaseState.Locked)
                return k;
        }

        // Si todo está balanceado, no hay fase activa
        return null;
    }

    bool IsPhaseBalanced(PhaseKey phase, Dictionary<string, int> imbalance)
    {
        foreach (var kv in imbalance)
        {
            string elem = kv.Key;
            int delta = kv.Value;
            if (delta == 0) continue;

            if (ElementBelongsToPhase(elem, phase))
                return false;
        }
        return true;
    }

    bool ElementBelongsToPhase(string symbol, PhaseKey phase)
    {
        if (phase == PhaseKey.Hydrogen) return symbol == "H";
        if (phase == PhaseKey.Oxygen) return symbol == "O";

        // Metales / NoMetales según tu DB
        var type = elementDatabase != null
            ? elementDatabase.GetTypeOrDefault(symbol, ElementType.NonMetal)
            : ElementType.NonMetal;

        if (phase == PhaseKey.Metals)
            return type == ElementType.Metal;

        if (phase == PhaseKey.NonMetals)
        {
            if (symbol == "H" || symbol == "O") return false;
            return type == ElementType.NonMetal;
        }

        return false;
    }

}