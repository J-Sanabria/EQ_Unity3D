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
    }

    public void ConfigureForReaction(BalanceStation st, Difficulty diff)
    {
        station = st;
        difficulty = diff;
        session = st != null ? st.session : null;

        if (session != null) session.CanAdjust = CanAdjust;

        BuildPhasePresence();
        InitStates();
        SelectInitialActivePhase();
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

        if (ShouldEnforceOrder())
        {
            if (_activePhase == null) _activePhase = key;
        }

        PushStateToHUDs();
        return true;
    }

    bool ShouldEnforceOrder()
    {
        return enforceOrderInTutorialAndEasy && (difficulty == Difficulty.Tutorial || difficulty == Difficulty.Easy);
    }

    // ---------- Permissions ----------
    bool CanAdjust(int side, int index, int delta)
    {
        if (station == null || station.reaction == null) return false;

        string formula = (side == 0) ? station.reaction.lhs[index] : station.reaction.rhs[index];
        PhaseKey phaseOfCompound = PhaseOfFormula(formula);

        if (_states.TryGetValue(phaseOfCompound, out var state))
        {
            if (state == PhaseState.Locked) return false;
            if (ShouldEnforceOrder() && _activePhase.HasValue && phaseOfCompound != _activePhase.Value) return false;
        }

        return true;
    }

    // ---------- Presence ----------
    void BuildPhasePresence()
    {
        _present.Clear();
        if (station == null || station.reaction == null) return;

        foreach (var f in station.reaction.lhs) _present.Add(PhaseOfFormula(f));
        foreach (var f in station.reaction.rhs) _present.Add(PhaseOfFormula(f));
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

    // ---------- Classification ----------
    PhaseKey PhaseOfFormula(string formula)
    {
        var atoms = ChemFormula.Parse(formula);

        bool hasH = atoms.ContainsKey("H");
        bool hasO = atoms.ContainsKey("O");

        // Si tiene metal -> Metales
        foreach (var e in atoms.Keys)
        {
            if (IsMetal(e)) return PhaseKey.Metals;
        }

        // Si es H puro u O puro
        if (atoms.Count == 1 && hasH) return PhaseKey.Hydrogen;
        if (atoms.Count == 1 && hasO) return PhaseKey.Oxygen;

        // Caso general: No metales
        return PhaseKey.NonMetals;
    }

    bool IsMetal(string symbol)
    {
        if (elementDatabase == null) return false;
        return elementDatabase.GetTypeOrDefault(symbol) == ElementType.Metal;
    }
}