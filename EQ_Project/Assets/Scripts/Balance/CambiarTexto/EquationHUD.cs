using UnityEngine;
using TMPro;
using System.Text;
using static ChemTextUtil;

public class EquationHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text text; // Asigna tu TMP del panel superior

    // Call: SetEquation(new[]{"H2","O2"}, new[]{"H2O"}, new[]{2,1}, new[]{2}, selectedSide:0, selectedIndex:0);

    public void SetEquation(string[] lhs, string[] rhs, int[] coefL, int[] coefR,
                            int selectedSide = -1, int selectedIndex = -1)
    {
        if (text == null) return;

        var sb = new StringBuilder(128);
        BuildSide(sb, lhs, coefL, selectedSide == 0 ? selectedIndex : -1);
        sb.Append("  \u2192  "); // flecha 
        BuildSide(sb, rhs, coefR, selectedSide == 1 ? selectedIndex : -1);

        text.richText = true;          // Asegúrate que el TMP usa Rich Text
        text.text = sb.ToString();
    }

    void BuildSide(StringBuilder sb, string[] species, int[] coefs, int highlightIndex)
    {
        for (int i = 0; i < species.Length; i++)
        {
            if (i > 0) sb.Append(" + ");

            int c = (coefs != null && i < coefs.Length) ? coefs[i] : 1;
            string f = ToTMPFormula(species[i]);

            if (highlightIndex == i)
            {
                // Resalta el término seleccionado durante el Balanceo
                sb.Append("<mark=#00000055>");
                if (c != 1) sb.Append("<b>").Append(c).Append("</b>").Append(' ');
                sb.Append("<b>").Append(f).Append("</b>");
                sb.Append("</mark>");
            }
            else
            {
                if (c != 1) sb.Append(c).Append(' ');
                sb.Append(f);
            }
        }
    }
}
