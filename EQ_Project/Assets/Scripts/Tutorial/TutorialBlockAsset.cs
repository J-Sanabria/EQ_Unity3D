using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ChemicalBalance/TutorialBlock", fileName = "TUT_BLOCK_")]
public class TutorialBlockAsset : ScriptableObject
{
    public string blockId = "intro";
    public List<TutorialStepAsset> steps = new();
}