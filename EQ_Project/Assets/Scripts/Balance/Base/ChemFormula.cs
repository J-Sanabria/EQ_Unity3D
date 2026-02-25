using System.Collections.Generic;
using System.Text;

namespace CB.Balance
{
    public static class ChemFormula
    {
        public static Dictionary<string, int> Parse(string formula)
        {
            int i = 0;
            var result = ParseGroup(formula, ref i);
            return result;
        }

        // Parse until end or ')'
        static Dictionary<string, int> ParseGroup(string s, ref int i)
        {
            var counts = new Dictionary<string, int>();

            while (i < s.Length)
            {
                char c = s[i];

                if (c == ')')
                {
                    i++; // consume ')'
                    break;
                }

                if (c == '(')
                {
                    i++; // consume '('
                    var sub = ParseGroup(s, ref i);   // parse inside
                    int k = ReadNumber(s, ref i);
                    if (k == 0) k = 1;
                    Merge(counts, sub, k);
                    continue;
                }

                if (char.IsUpper(c))
                {
                    string elem = ReadElement(s, ref i);
                    int k = ReadNumber(s, ref i);
                    if (k == 0) k = 1;

                    if (!counts.ContainsKey(elem)) counts[elem] = 0;
                    counts[elem] += k;
                    continue;
                }

                // ignore unexpected
                i++;
            }

            return counts;
        }

        static string ReadElement(string s, ref int i)
        {
            var sb = new StringBuilder();
            sb.Append(s[i]); // upper
            i++;
            if (i < s.Length && char.IsLower(s[i]))
            {
                sb.Append(s[i]);
                i++;
            }
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