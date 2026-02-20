using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;
using CB.Balance;

public class EquationHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text text;

    [Header("Colores")]
    [SerializeField] string badHex = "#FF5555";

    ReactionAsset current;

    public void SetEquation(
        string[] lhs,
        string[] rhs,
        int[] coefL,
        int[] coefR,
        int selectedSide = -1,
        int selectedIndex = -1,
        HashSet<string> badElements = null)
    {
        if (text == null || lhs == null || rhs == null)
            return;

        var sb = new StringBuilder(128);

        BuildSide(sb, lhs, coefL, selectedSide == 0 ? selectedIndex : -1, badElements);
        sb.Append("  \u2192  ");
        BuildSide(sb, rhs, coefR, selectedSide == 1 ? selectedIndex : -1, badElements);

        text.richText = true;
        text.text = sb.ToString();
    }

    public void Clear()
    {
        if (text != null)
            text.text = string.Empty;
    }

    void BuildSide(
        StringBuilder sb,
        string[] species,
        int[] coefs,
        int highlightIndex,
        HashSet<string> badElements)
    {
        for (int i = 0; i < species.Length; i++)
        {
            if (i > 0) sb.Append(" + ");

            int coef = (coefs != null && i < coefs.Length) ? Mathf.Max(1, coefs[i]) : 1;
            string formula = ChemTextUtil.ToTMPFormula(species[i]);

            bool isBad = IsSpeciesBad(species[i], badElements);

            if (highlightIndex == i)
            {
                sb.Append("<mark=#00000055>");
                AppendSpecies(sb, coef, formula, isBad, bold: true);
                sb.Append("</mark>");
            }
            else
            {
                AppendSpecies(sb, coef, formula, isBad, bold: false);
            }
        }
    }

    void AppendSpecies(
        StringBuilder sb,
        int coef,
        string formula,
        bool isBad,
        bool bold)
    {
        if (coef != 1)
            sb.Append(bold ? "<b>" + coef + "</b> " : coef + " ");

        if (isBad)
            sb.Append("<color=").Append(badHex).Append('>');

        if (bold) sb.Append("<b>");
        sb.Append(formula);
        if (bold) sb.Append("</b>");

        if (isBad)
            sb.Append("</color>");
    }

    bool IsSpeciesBad(string species, HashSet<string> badElements)
    {
        if (badElements == null || badElements.Count == 0)
            return false;

        var parsed = ChemFormula.Parse(species);
        foreach (var elem in parsed.Keys)
            if (badElements.Contains(elem))
                return true;

        return false;
    }
}
