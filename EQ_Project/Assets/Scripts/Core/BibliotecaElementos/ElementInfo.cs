using System;
using UnityEngine;

public enum ElementType
{
    Metal,
    NonMetal
}
[Serializable]
public class ElementInfo
{
    public string symbol;          // "Na"
    public ElementType type;       // Metal/NonMetal
    public Color cpkColor = Color.white; // opcional
}