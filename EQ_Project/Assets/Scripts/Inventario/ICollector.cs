using UnityEngine;

public interface ICollector
{
    // Devuelve true si el objeto fue aceptado (por ejemplo, si había espacio)
    bool Collect(string itemId, int amount, Transform source);
}
