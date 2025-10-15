using UnityEngine;

namespace CB.Balance
{
    public class BalanceSessionController : MonoBehaviour, IBalanceUIScreen
    {
        [Header("UI")]
        [SerializeField] EquationHUD equationHUD;

        [Header("Estado")]
        public int[] coefL;
        public int[] coefR;

        // NUEVO: contador de errores de verificación
        [Header("Progreso")]
        public int errorCount = 0;

        BalanceStation station;

        public void BindStation(BalanceStation s)
        {
            station = s;
            InitFromStation();
            Render();
            errorCount = 0;
        }

        void InitFromStation()
        {
            if (station == null || station.reaction == null) { coefL = coefR = null; return; }
            var rxn = station.reaction;
            coefL = (int[])rxn.coefL.Clone();
            coefR = (int[])rxn.coefR.Clone();
        }

        public void Adjust(int side, int index, int delta)
        {
            if (side == 0 && coefL != null && index >= 0 && index < coefL.Length)
            {
                coefL[index] = Mathf.Max(0, coefL[index] + delta);
            }
            else if (side == 1 && coefR != null && index >= 0 && index < coefR.Length)
            {
                coefR[index] = Mathf.Max(0, coefR[index] + delta);
            }
            Render();
        }

        public bool IsBalancedNow()
        {
            if (station == null || station.reaction == null) return false;
            var rxn = station.reaction;
            return ReactionValidator.IsBalanced(rxn.lhs, rxn.rhs, coefL, coefR);
        }

        public void Render(int selectedSide = -1, int selectedIndex = -1)
        {
            if (station == null || station.reaction == null || equationHUD == null) return;
            var rxn = station.reaction;
            equationHUD.SetEquation(rxn.lhs, rxn.rhs, coefL, coefR, selectedSide, selectedIndex);
        }

        // NUEVO: helpers para contar términos por lado
        public int LeftCount { get { return station != null && station.reaction != null && station.reaction.lhs != null ? station.reaction.lhs.Length : 0; } }
        public int RightCount { get { return station != null && station.reaction != null && station.reaction.rhs != null ? station.reaction.rhs.Length : 0; } }
    }
}

