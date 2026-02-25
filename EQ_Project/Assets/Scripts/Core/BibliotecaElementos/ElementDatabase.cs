using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ChemicalBalance/ElementDatabase")]
public class ElementDatabase : ScriptableObject
{
    public List<ElementInfo> elements = new();

    private Dictionary<string, ElementInfo> _map;

    public void BuildCache()
    {
        _map = new Dictionary<string, ElementInfo>();
        foreach (var e in elements)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.symbol)) continue;
            _map[e.symbol.Trim()] = e;
        }
    }

    public bool TryGet(string symbol, out ElementInfo info)
    {
        if (_map == null) BuildCache();
        return _map.TryGetValue(symbol, out info);
    }

    public ElementType GetTypeOrDefault(string symbol, ElementType def = ElementType.NonMetal)
    {
        return TryGet(symbol, out var info) ? info.type : def;
    }
}