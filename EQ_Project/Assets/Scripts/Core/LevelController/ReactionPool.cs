using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ChemicalBalance/ReactionPool")]
public class ReactionPool : ScriptableObject
{
    public Difficulty difficulty;
    public List<ReactionAsset> reactions; // ej: 6
}
