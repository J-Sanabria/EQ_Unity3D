using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;
using CB.Balance; // para ChemFormula.Parse

public class EquationHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text text;

    [Header("Colores")]
    [SerializeField] string badHex = "#FF5555";   // color para especies con elementos desbalanceados

    // badElements: elementos cuyo balance  0 (ej. { "H", "O" })
    public void SetEquation(string[] lhs, string[] rhs, int[] coefL, int[] coefR,
                            int selectedSide = -1, int selectedIndex = -1,
                            HashSet<string> badElements = null)
    {
        if (text == null) return;

        var sb = new StringBuilder(128);
        BuildSide(sb, lhs, coefL, selectedSide == 0 ? selectedIndex : -1, badElements);
        sb.Append("  \u2192  ");
        BuildSide(sb, rhs, coefR, selectedSide == 1 ? selectedIndex : -1, badElements);

        text.richText = true;
        text.text = sb.ToString();
    }

    void BuildSide(StringBuilder sb, string[] species, int[] coefs, int highlightIndex, HashSet<string> badElements)
    {
        for (int i = 0; i < species.Length; i++)
        {
            if (i > 0) sb.Append(" + ");

            int c = (coefs != null && i < coefs.Length) ? coefs[i] : 1;
            string formula = ChemTextUtil.ToTMPFormula(species[i]);

            bool isBad = SpeciesTouchesBad(species[i], badElements);

            if (highlightIndex == i)
            {
                sb.Append("<mark=#00000055>");
                if (c != 1) sb.Append("<b>").Append(c).Append("</b>").Append(' ');
                if (isBad) sb.Append("<color=").Append(badHex).Append('>');
                sb.Append("<b>").Append(formula).Append("</b>");
                if (isBad) sb.Append("</color>");
                sb.Append("</mark>");
            }
            else
            {
                if (c != 1) sb.Append(c).Append(' ');
                if (isBad) sb.Append("<color=").Append(badHex).Append('>');
                sb.Append(formula);
                if (isBad) sb.Append("</color>");
            }
        }
    }

    bool SpeciesTouchesBad(string species, HashSet<string> badElements)
    {
        if (badElements == null || badElements.Count == 0 || string.IsNullOrEmpty(species))
            return false;

        var elems = ChemFormula.Parse(species);
        foreach (var k in elems.Keys)
            if (badElements.Contains(k)) return true;

        return false;
    }
}
