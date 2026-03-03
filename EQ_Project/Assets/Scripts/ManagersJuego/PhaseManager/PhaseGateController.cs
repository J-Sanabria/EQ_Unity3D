using System;
using System.Collections.Generic;
using UnityEngine;

public class PhaseGateController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] PhaseManager phaseManager;

    [Header("Gates")]
    [SerializeField] List<PhaseGate> gates = new();

    [Header("Rules")]
    [SerializeField] bool onlyTutorialAndEasy = true;

    static readonly PhaseKey[] Order = {
        PhaseKey.Metals, PhaseKey.NonMetals, PhaseKey.Hydrogen, PhaseKey.Oxygen
    };

    HashSet<PhaseKey> _present = new();
    Difficulty _difficulty;

    void Reset()
    {
        if (phaseManager == null) phaseManager = FindFirstObjectByType<PhaseManager>();
    }

    public void Configure(HashSet<PhaseKey> present, Difficulty difficulty)
    {
        _present = present != null ? new HashSet<PhaseKey>(present) : new HashSet<PhaseKey>();
        _difficulty = difficulty;

        bool useGates = !onlyTutorialAndEasy || (difficulty == Difficulty.Tutorial || difficulty == Difficulty.Easy);

        // Si no se usan puertas, todo abierto
        if (!useGates)
        {
            foreach (var g in gates)
                if (g != null) g.SetMode(PhaseGate.GateMode.Open, instant: true);
            return;
        }

        // En tutorial/fácil:
        // - fases not present: NotPresent (no bloquea)
        // - primera fase presente: Open
        // - resto presentes: Locked
        PhaseKey? first = FirstPresentPhase();

        foreach (var g in gates)
        {
            if (g == null) continue;

            if (!_present.Contains(g.Phase))
            {
                g.SetMode(PhaseGate.GateMode.NotPresent, instant: true);
                continue;
            }

            if (first.HasValue && g.Phase == first.Value)
                g.SetMode(PhaseGate.GateMode.Open, instant: true);
            else
                g.SetMode(PhaseGate.GateMode.Locked, instant: true);
        }

        Debug.Log($"[Gates] diff={difficulty} present={string.Join(",", _present)} first={FirstPresentPhase()}");
    }

    public void OnKeyPicked(PhaseKey pickedKey)
    {
        bool useGates = !onlyTutorialAndEasy || (_difficulty == Difficulty.Tutorial || _difficulty == Difficulty.Easy);
        if (!useGates) return;

        // Cuando recoja una llave, abrir la puerta de la siguiente fase presente
        PhaseKey? next = NextPresentPhaseAfter(pickedKey);
        if (!next.HasValue) return;

        var gate = FindGate(next.Value);
        if (gate != null)
            gate.SetMode(PhaseGate.GateMode.Open, instant: false);
    }

    PhaseGate FindGate(PhaseKey k)
    {
        for (int i = 0; i < gates.Count; i++)
            if (gates[i] != null && gates[i].Phase == k)
                return gates[i];
        return null;
    }

    PhaseKey? FirstPresentPhase()
    {
        for (int i = 0; i < Order.Length; i++)
            if (_present.Contains(Order[i]))
                return Order[i];
        return null;
    }

    PhaseKey? NextPresentPhaseAfter(PhaseKey current)
    {
        int idx = Array.IndexOf(Order, current);
        for (int i = idx + 1; i < Order.Length; i++)
            if (_present.Contains(Order[i]))
                return Order[i];
        return null;
    }
}