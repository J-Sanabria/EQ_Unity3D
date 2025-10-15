using System.Collections.Generic;
using System.Text;

namespace CB.Balance
{
    public static class ChemFormula
    {
        // Parsea "H2SO4", "Ca(OH)2", "Fe2(SO4)3", "Al2(SO4)3"...
        // Soporta paréntesis anidados.
        public static Dictionary<string, int> Parse(string formula)
        {
            var i = 0;
            return ParseGroup(formula, ref i, 1);
        }

        static Dictionary<string, int> ParseGroup(string s, ref int i, int mult)
        {
            var counts = new Dictionary<string, int>();

            while (i < s.Length)
            {
                char c = s[i];

                if (c == ')') // fin de grupo
                {
                    i++;
                    int k = ReadNumber(s, ref i);
                    MultiplyInto(counts, mult * (k == 0 ? 1 : k));
                    return counts;
                }
                else if (c == '(') // subgrupo
                {
                    i++;
                    var sub = ParseGroup(s, ref i, 1);
                    Merge(counts, sub, 1);
                }
                else if (char.IsUpper(c)) // elemento
                {
                    string elem = ReadElement(s, ref i); // avanza i
                    int k = ReadNumber(s, ref i);
                    int add = (k == 0 ? 1 : k);
                    if (!counts.ContainsKey(elem)) counts[elem] = 0;
                    counts[elem] += add;
                }
                else
                {
                    // caracter inesperado, avanza para evitar bucle
                    i++;
                }
            }

            MultiplyInto(counts, mult);
            return counts;
        }

        static string ReadElement(string s, ref int i)
        {
            var sb = new StringBuilder();
            sb.Append(s[i]); // Upper ya validada
            i++;
            if (i < s.Length && char.IsLower(s[i])) { sb.Append(s[i]); i++; }
            return sb.ToString();
        }

        static int ReadNumber(string s, ref int i)
        {
            int value = 0;
            int start = i;
            while (i < s.Length && char.IsDigit(s[i]))
            {
                value = value * 10 + (s[i] - '0');
                i++;
            }
            return i == start ? 0 : value;
        }

        static void MultiplyInto(Dictionary<string, int> counts, int m)
        {
            if (m == 1) return;
            var keys = new List<string>(counts.Keys);
            for (int k = 0; k < keys.Count; k++)
                counts[keys[k]] *= m;
        }

        static void Merge(Dictionary<string, int> into, Dictionary<string, int> from, int mult)
        {
            foreach (var kv in from)
            {
                int add = kv.Value * mult;
                if (!into.ContainsKey(kv.Key)) into[kv.Key] = 0;
                into[kv.Key] += add;
            }
        }
    }
}
