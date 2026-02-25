public enum PhaseState
{
    NotPresent,    // la ecuación no lo usa
    Locked,        // existe, pero no tienes llave
    Unlocked,      // tienes llave
    Completed      // fase completada (sellada en tutorial/fácil)
}