using System.Text.RegularExpressions;

public static class ChemTextUtil
{
    // Envuelve todos los dígitos de la fórmula en <sub>...</sub>
    // Ej: "H2SO4" -> "H<sub>2</sub>SO<sub>4</sub>"
    // Nota: pasa solo la fórmula SIN coeficiente (ej "H2O", no "2 H2O")
    static readonly Regex rxDigits = new Regex(@"\d+");

    public static string ToTMPFormula(string formula)
    {
        if (string.IsNullOrEmpty(formula)) return "";
        return rxDigits.Replace(formula, m => "<sub>" + m.Value + "</sub>");
    }

    // Opcional: estado físico en pequeño/itálica: (aq), (s), (g), (l)
    public static string FormatState(string state) // "aq", "s", "l", "g" o null
    {
        if (string.IsNullOrEmpty(state)) return "";
        return " <size=70%><i>(" + state + ")</i></size>";
    }
}
