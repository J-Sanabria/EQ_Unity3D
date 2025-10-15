using System.Collections.Generic;

namespace CB.Balance
{
    public static class ReactionValidator
    {
        // Devuelve conteo de átomos por lado
        public static Dictionary<string, int> CountSide(string[] species, int[] coef)
        {
            var total = new Dictionary<string, int>();
            if (species == null) return total;

            for (int i = 0; i < species.Length; i++)
            {
                int c = (coef != null && i < coef.Length) ? coef[i] : 1;
                if (c <= 0) c = 0;

                var atoms = ChemFormula.Parse(species[i] ?? "");
                foreach (var kv in atoms)
                {
                    int add = kv.Value * c;
                    if (!total.ContainsKey(kv.Key)) total[kv.Key] = 0;
                    total[kv.Key] += add;
                }
            }
            return total;
        }

        // Diferencia L - R para cada elemento
        public static Dictionary<string, int> Imbalance(string[] lhs, string[] rhs, int[] coefL, int[] coefR)
        {
            var L = CountSide(lhs, coefL);
            var R = CountSide(rhs, coefR);

            // union de claves
            var all = new HashSet<string>(L.Keys);
            all.UnionWith(R.Keys);

            var diff = new Dictionary<string, int>();
            foreach (var e in all)
            {
                int l = L.ContainsKey(e) ? L[e] : 0;
                int r = R.ContainsKey(e) ? R[e] : 0;
                diff[e] = l - r; // 0 significa balanceado para ese elemento
            }
            return diff;
        }

        public static bool IsBalanced(string[] lhs, string[] rhs, int[] coefL, int[] coefR)
        {
            var d = Imbalance(lhs, rhs, coefL, coefR);
            foreach (var kv in d) if (kv.Value != 0) return false;
            return true;
        }
    }
}
