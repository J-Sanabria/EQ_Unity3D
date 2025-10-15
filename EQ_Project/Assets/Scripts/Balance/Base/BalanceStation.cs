using UnityEngine;

namespace CB.Balance
{
    public class BalanceStation : MonoBehaviour
    {
        public ReactionAsset reaction;          // asigna aquí la ecuación de esta estación
        public Transform cameraFocus;           // opcional, para Cinemachine

        public string ReactionId => reaction ? reaction.reactionId : "";
        public string[] LHS => reaction ? reaction.lhs : null;
        public string[] RHS => reaction ? reaction.rhs : null;
        public int[] CoefL => reaction ? reaction.coefL : null;
        public int[] CoefR => reaction ? reaction.coefR : null;

        public BalanceSessionController sessionUI;        // arrastra el del PanelEcuacionBalance
        public BalanceVisualController balanceVisual;     // arrastra el de la balanza

        void OnValidate()
        {
            // si ambas existen, vincula
            if (balanceVisual != null && sessionUI != null)
                balanceVisual.BindSession(sessionUI);
        }
    }
}
