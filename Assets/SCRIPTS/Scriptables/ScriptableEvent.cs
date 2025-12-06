
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableEvent", order = 1)]
public class ScriptableEvent : ScriptableObject
{
    //Th
    public string EventController; //ID for the EventController which runs the proper code.

    public ScriptableEnemySpawner EnemyWave;

    public List<ScriptableDialog> AdditionalEventDialogs; //Additional dialogs that could be played. These are triggered by events.
    public ScriptableLootTable LootTable;

    public bool HasDebrief()
    {
        return DebriefDialog != null;
    }

    public AlternativeDebriefDialog GetDebrief()
    {
        foreach (AlternativeDebriefDialog alternativeDialog in AlternativeDebriefs)
        {
            if (alternativeDialog.ArePrerequisitesMet()) return alternativeDialog;
        }
        AlternativeDebriefDialog dialog = new AlternativeDebriefDialog();
        dialog.ReplaceDialog = DebriefDialog;
        dialog.AlternativeLoot = LootTable;
        return dialog;
    }

    public ScriptableDialog DebriefDialog; //Dialog usually played at the end of the event.
    public List<AlternativeDebriefDialog> AlternativeDebriefs;
}
[Serializable]
public class AlternativeDebriefDialog //This dialog will be played only if its prerequisites are met.
{
    public ScriptableDialog ReplaceDialog;
    public ScriptablePrerequisite[] Prerequisites;
    public ScriptableLootTable AlternativeLoot;
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
