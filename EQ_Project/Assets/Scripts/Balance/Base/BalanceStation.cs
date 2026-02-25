using UnityEngine;

namespace CB.Balance
{
    public class BalanceStation : MonoBehaviour
    {
        [Header("Data")]
        public ReactionAsset reaction;

        [Header("Refs")]
        public BalanceSessionController session;
        public BalanceSelectionController selection;
        public BalanceVisualController visual;
        public Transform cameraFocus;

        public string ReactionId => reaction != null ? reaction.reactionId : null;

        void Awake()
        {
            if (visual != null && session != null)
                visual.BindSession(session);
        }

        void OnValidate()
        {
            if (visual != null && session != null)
                visual.BindSession(session);
        }
    }
}