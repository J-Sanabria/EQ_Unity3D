using UnityEngine;

[CreateAssetMenu(menuName = "ChemicalBalance/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public Difficulty difficulty;
    public ReactionPool reactionPool;
    public int reactionsPerRun = 3;
}