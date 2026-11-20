using UnityEngine;

public enum TutorialTriggerMoment
{
    StartOfBattle,
    MidBattle,
    EndOfBattle
}

[System.Serializable]
public struct TutorialStep
{
    public string speakerOrTitle;
    [TextArea(3, 6)]
    public string message;
}

[CreateAssetMenu(fileName = "NewTutorialSequence", menuName = "Game/Tutorial Sequence")]
public class TutorialSequence : ScriptableObject
{
    public string tutorialId = "tut_intro";
    public TutorialTriggerMoment moment = TutorialTriggerMoment.StartOfBattle;
    public bool pauseGameWhileOpen = true;
    public bool showOnlyOnce = true;
    public TutorialStep[] steps;
}

/*
 * if (TutorialManager.Instance != null)
{
    TutorialManager.Instance.PlayById("tut_first_capture");
}
*/