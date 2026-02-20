using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class ChemicalFormulaParser
{
    // Coincide con símbolos químicos: H, He, Na, Cl, etc.
    static Regex elementRegex = new Regex(@"[A-Z][a-z]?");

    public static HashSet<string> ExtractElements(string formula)
    {
        HashSet<string> elements = new HashSet<string>();

        if (string.IsNullOrEmpty(formula))
            return elements;

        var matches = elementRegex.Matches(formula);

        foreach (Match m in matches)
        {
            elements.Add(m.Value);
        }

        return elements;
    }
}
