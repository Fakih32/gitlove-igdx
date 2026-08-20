using UnityEngine;

[CreateAssetMenu(fileName = "New Cutscene", menuName = "Cutscene/Cutscene Sequence")]
public class CutsceneSequenceSO : ScriptableObject
{
    public ComicPanelSO[] panels;
    public bool skippable = true;
}

