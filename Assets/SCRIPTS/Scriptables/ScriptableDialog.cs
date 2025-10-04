using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableDialog", order = 1)]
public class ScriptableDialog : ScriptableObject
{
    public DialogPart[] StoryTexts; //All the parts of dialog of text...
   
    public PossibleNextDialog[] ChoicePathDialogs; //All the possible choices you can go into...
}

[Serializable]
public struct DialogPart //This is a single line in the dialog. It can be replaced based on alternatives.
{
    [TextArea(3, 10)]
    public string BaseText;
    public ScriptableDialogSpeaker Speaker;
    public AlternativeDialogPart[] Alternatives;
}

[Serializable]
public struct PossibleNextDialog //This possible next dialog has a descriptive text, and leads to a new dialog. May be based on alternatives.
{
    public DialogPart ChoiceText;

    [Header("CONTINUED DIALOG ONLY")]
    public AlternativePrerequisite[] Prerequisites;
    public ScriptableDialog DialogResult;
    public AlternativeDialog[] AlternativeResults;

    [Header("END COMMUNICATIONS ONLY")]
    public ScriptableEvent TriggerEvent;
}

[Serializable]
public struct AlternativeDialog //This dialog will be played only if its prerequisites are met.
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
public struct AlternativeDialogPart //This alternative piece of text happens only if prerequisites are met.
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
public class AlternativePrerequisiteReputation : AlternativePrerequisite //Only if your reputation for target faction is between two values...
{
    public int MinReputation = -999;
    public int MaxReputation = 999;
    public CO.Faction FactionID;
    public override bool IsTrue()
    {
        return CO.co.Resource_Reputation.GetValueOrDefault(FactionID) > MinReputation && CO.co.Resource_Reputation.GetValueOrDefault(FactionID) < MaxReputation;
    }
}
[Serializable]
public class AlternativePrerequisiteBackground : AlternativePrerequisite //Only if you have a character in the party with a certain background...
{
    public List<ScriptableBackground> Backgrounds;
    public override bool IsTrue()
    {
        return true;
    }
}
[Serializable]
public class AlternativePrerequisiteRandom : AlternativePrerequisite //Only if the chance is met...
{
    public float Chance;
    public override bool IsTrue()
    {
        return UnityEngine.Random.Range(0f,1f) < Chance;
    }
}
[Serializable]
public class AlternativePrerequisiteAND : AlternativePrerequisite //Only if all other alternatives in this list are true...
{
    public List<AlternativePrerequisite> Alternatives;
    public override bool IsTrue()
    {
        foreach (var a in Alternatives)
        {
            if (!a.IsTrue()) return false;
        }
        return true;
    }
}
[Serializable]
public class AlternativePrerequisiteOR : AlternativePrerequisite //Only if at least one other alternative in this list is true...
{
    public List<AlternativePrerequisite> Alternatives;
    public override bool IsTrue()
    {
        foreach (var a in Alternatives)
        {
            if (a.IsTrue()) return true;
        }
        return false;
    }
}