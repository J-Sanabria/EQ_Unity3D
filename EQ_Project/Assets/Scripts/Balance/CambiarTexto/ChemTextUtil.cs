using System.Collections.Generic;
using System.Text;

public static class ChemTextUtil
{
    // Convierte "H2O" a "H<sub>2</sub><color=#FF5555>O</color>"
    // Colorea SOLO los símbolos de elementos que están en badElements.
    // Maneja paréntesis y números: "Ca(OH)2" -> Ca(O H)2 con subscripts correctos.
    public static string ToTMPFormulaColored(string formula, HashSet<string> badElements, string badHex)
    {
        if (string.IsNullOrEmpty(formula)) return "";

        var sb = new StringBuilder(formula.Length * 2);
        int i = 0;

        while (i < formula.Length)
        {
            char c = formula[i];

            // Paréntesis
            if (c == '(' || c == ')')
            {
                sb.Append(c);
                i++;
                continue;
            }

            // Elemento: Upper + opcional lower
            if (char.IsUpper(c))
            {
                string elem = ReadElement(formula, ref i); // avanza i

                bool isBad = badElements != null && badElements.Contains(elem);
                if (isBad) sb.Append("<color=").Append(badHex).Append(">");

                sb.Append(elem);

                if (isBad) sb.Append("</color>");

                // Número después del elemento (subíndice)
                AppendSubNumber(formula, ref i, sb);
                continue;
            }

            // Número suelto (por ejemplo después de ')'): "(OH)2"
            if (char.IsDigit(c))
            {
                AppendSubNumber(formula, ref i, sb);
                continue;
            }

            // Cualquier otro char raro: lo copias
            sb.Append(c);
            i++;
        }

        return sb.ToString();
    }

    static string ReadElement(string s, ref int i)
    {
        // s[i] es Upper
        var sb = new StringBuilder(2);
        sb.Append(s[i]);
        i++;

        if (i < s.Length && char.IsLower(s[i]))
        {
            sb.Append(s[i]);
            i++;
        }

        return sb.ToString();
    }

    static void AppendSubNumber(string s, ref int i, StringBuilder sb)
    {
        int start = i;
        while (i < s.Length && char.IsDigit(s[i])) i++;

        if (i > start)
        {
            sb.Append("<sub>");
            sb.Append(s, start, i - start);
            sb.Append("</sub>");
        }
    }
}