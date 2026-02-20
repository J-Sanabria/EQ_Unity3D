using UnityEngine;
using System.Collections.Generic;
using CB.Balance;

public class EquationHUDBinding : MonoBehaviour
{
    [Header("HUDs")]
    [SerializeField] EquationHUD explorationHUD;
    [SerializeField] EquationHUD balanceHUD;

    BalanceSessionController session;
    BalanceSelectionController selection;

    ReactionAsset reaction;

    void OnEnable()
    {
        if (selection != null)
            selection.OnSelectionChanged += OnSelectionChanged;

        if (session != null)
            session.OnEquationChanged += Refresh;
    }

    void OnDisable()
    {
        if (selection != null)
            selection.OnSelectionChanged -= OnSelectionChanged;

        if (session != null)
            session.OnEquationChanged -= Refresh;
    }

    public void Bind(
        BalanceSessionController s,
        BalanceSelectionController sel)
    {
        session = s;
        selection = sel;

        if (selection != null)
            selection.OnSelectionChanged += OnSelectionChanged;

        if (session != null)
            session.OnEquationChanged += Refresh;

        reaction = session?.Station?.reaction;
        Refresh();
    }

    public void SetReaction(ReactionAsset rxn)
    {
        reaction = rxn;
        Refresh();
    }

    void OnSelectionChanged(int _, int __)
    {
        Refresh();
    }

    void Refresh()
    {
        if (reaction == null)
            return;

        int[] coefL = session != null ? session.coefL : reaction.coefL;
        int[] coefR = session != null ? session.coefR : reaction.coefR;

        HashSet<string> badElements = null;

        if (session != null)
        {
            var bad = ReactionValidator.Imbalance(
                reaction.lhs,
                reaction.rhs,
                coefL,
                coefR
            );

            badElements = new HashSet<string>();
            foreach (var kv in bad)
                if (kv.Value != 0)
                    badElements.Add(kv.Key);
        }

        Render(explorationHUD, highlight: false);
        Render(balanceHUD, highlight: true);

        void Render(EquationHUD hud, bool highlight)
        {
            if (hud == null) return;

            hud.SetEquation(
                reaction.lhs,
                reaction.rhs,
                coefL,
                coefR,
                highlight && selection != null ? selection.SelectedSide : -1,
                highlight && selection != null ? selection.SelectedIndex : -1,
                badElements
            );
        }
    }

}
