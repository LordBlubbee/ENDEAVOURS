
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableEvent", order = 1)]
public class ScriptableEvent : ScriptableObject
{
    //Th
    public AudioClip BeginEventOST; //OST played when this event begins.
    public string EventController; //ID for the EventController which runs the proper code.

    public ScriptableEnemySpawner EnemyWave;

    public List<ScriptableDialog> AdditionalDialogs; //Additional dialogs that could be played.
    public ScriptableLootTable LootTable;
    public ScriptableDialog DebriefDialog; //Dialog usually played at the end of the event.
}
