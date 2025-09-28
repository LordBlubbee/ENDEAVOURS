using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableDialog", order = 1)]
public class ScriptableDialog : ScriptableObject
{
    public DialogPart[] StoryTexts;
   
    public string TriggerEvent;
    public DialogPart[] ChoiceTexts;
    public ScriptableDialog[] ChoicePathDialogs;
    public AlternativeDialog[] AlternativeDialogs;
}

[Serializable]
public struct DialogPart
{
    [TextArea(3, 10)]
    public string BaseText;
    public ScriptableDialogSpeaker Speaker;
    public AlternativeDialogPart[] Alternatives;
}

[Serializable]
public struct AlternativeDialog
{
    public ScriptableDialog ReplaceDialog;
    public AlternativePrerequisite[] Prerequisites;
    public bool ArePrerequisitesMet()
    {
        foreach (var prerequisite in Prerequisites)
        {
            if (!prerequisite.IsTrue())
            {
                return false;
            }
        }
        return true;
    }
}

[Serializable]
public struct AlternativeDialogPart
{
    [TextArea(3, 10)]
    public string AlternativeText;
    public AlternativePrerequisite[] Prerequisites;
    public bool ArePrerequisitesMet()
    {
        foreach (var prerequisite in Prerequisites)
        {
            if (!prerequisite.IsTrue())
            {
                return false;
            }
        }
        return true;
    }
}

[Serializable]
public abstract class AlternativePrerequisite
{
    public abstract bool IsTrue();
}

[Serializable]
public class AlternativePrerequisiteReputation : AlternativePrerequisite
{
    public int MinReputation;
    public int FactionID;
    public override bool IsTrue()
    {
        return false; // Placeholder
    }
}