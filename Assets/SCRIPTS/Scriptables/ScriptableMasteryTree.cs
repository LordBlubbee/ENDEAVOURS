using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MasteryTree", order = 1)]
public class ScriptableMasteryTree : ScriptableObject
{
    public string ItemName;
    public string ItemTagline;
    public Color ThemeColor;
    [TextArea(3, 10)]
    public string ItemDesc;
    public Sprite ItemIcon;

    public List<ScriptableMasteryItem> MasteryItems;
    /*
       - 0 1 2
       - 6 7 8 9
       - 3 4 5
     */
}
