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
    public ScriptablePrerequisite[] Prerequisites;
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

    [Header("CONTINUED DIALOG ONLY")]
    public ScriptableDialog DialogResult;
    public AlternativeDialog[] AlternativeResults;

    [Header("END COMMUNICATIONS ONLY")]
    public ScriptableEvent TriggerEvent;
}

[Serializable]
public struct AlternativeDialog //This dialog will be played only if its prerequisites are met.
{
    public ScriptableDialog ReplaceDialog;
    public ScriptablePrerequisite[] Prerequisites;
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
    public ScriptablePrerequisite[] Prerequisites;
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