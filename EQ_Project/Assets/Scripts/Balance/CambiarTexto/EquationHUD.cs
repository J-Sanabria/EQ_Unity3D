using System.Text;
using System.Collections.Generic;
using CB.Balance;
using TMPro;
using UnityEngine;

public class EquationHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text text;

    [Header("Colores")]
    [SerializeField] string badHex = "#FF5555";

    public void SetEquation(
        string[] lhs,
        string[] rhs,
        int[] coefL,
        int[] coefR,
        int selectedSide = -1,
        int selectedIndex = -1,
        HashSet<string> badElements = null)
    {
        if (text == null || lhs == null || rhs == null) return;

        var sb = new StringBuilder(128);

        BuildSide(sb, lhs, coefL, selectedSide == 0 ? selectedIndex : -1, badElements);
        sb.Append(" <size=140%><b> \u2192 </b></size> ");
        BuildSide(sb, rhs, coefR, selectedSide == 1 ? selectedIndex : -1, badElements);

        text.richText = true;
        text.text = sb.ToString();
    }

    public void Clear()
    {
        if (text != null) text.text = string.Empty;
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

            // ahora se colorea POR ELEMENTO dentro de la fórmula
            string formula = ChemTextUtil.ToTMPFormulaColored(species[i], badElements, badHex);

            if (highlightIndex == i)
            {
                sb.Append("<mark=#00000055>");
                AppendSpecies(sb, coef, formula, bold: true);
                sb.Append("</mark>");
            }
            else
            {
                AppendSpecies(sb, coef, formula, bold: false);
            }
        }
    }

    void AppendSpecies(StringBuilder sb, int coef, string formula, bool bold)
    {
        if (coef != 1)
            sb.Append(bold ? "<b>" + coef + "</b> " : coef + " ");

        if (bold) sb.Append("<b>");
        sb.Append(formula);
        if (bold) sb.Append("</b>");
    }
}