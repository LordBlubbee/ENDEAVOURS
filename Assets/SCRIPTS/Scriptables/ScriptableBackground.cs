
using System;
using System.Collections.Generic;
using UnityEngine;
using static CO;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnScriptableBackground", order = 1)]
public class ScriptableBackground : ScriptableObject
{
    public string BackgroundName;
    public string ResourcePath;
    public Sprite MainIcon;
    public Sprite Sprite_Player;
    public Sprite Sprite_Stripes;
    [TextArea(3, 10)]
    public string ShortDesc;
    [TextArea(5, 10)]
    public string LongDesc;
    public Color BackgroundColor;
    [Header("PHYS, ARM, DEX, COM, HAR, ENG, GUN, MED")]
    public int[] Background_ATT_BONUS = new int[8];
    public ScriptableEquippableWeapon Background_StartingWeapon;
    public List<FactionReputation> Background_ReputationEffect;
}
[Serializable]
public struct FactionReputation
{
    public Faction Fac;
    public int Amount;
}