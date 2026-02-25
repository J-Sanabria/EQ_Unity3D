public interface IPhaseHUD
{
    void SetPhaseState(PhaseKey key, PhaseState state);
    void SetActivePhase(PhaseKey? key);
}