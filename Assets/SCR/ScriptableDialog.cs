using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableDialog", order = 1)]
public class ScriptableDialog : ScriptableObject
{
    public DialogPart[] StoryTexts;
   
    public string TriggerEvent;
    public PossibleNextDialog[] ChoicePathDialogs;
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
public struct PossibleNextDialog
{
    public DialogPart ChoiceText;
    public AlternativePrerequisite[] Prerequisites;
    public ScriptableDialog DialogResult;
    public AlternativeDialog[] AlternativeResults;
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
    public int MinReputation = -999;
    public int MaxReputation = 999;
    public CO.Faction FactionID;
    public override bool IsTrue()
    {
        return CO.co.Resource_Reputation.GetValueOrDefault(FactionID) > MinReputation && CO.co.Resource_Reputation.GetValueOrDefault(FactionID) < MaxReputation;
    }
}
[Serializable]
public class AlternativePrerequisiteRandom : AlternativePrerequisite
{
    public float Chance;
    public override bool IsTrue()
    {
        return UnityEngine.Random.Range(0f,1f) < Chance;
    }
}