using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
public class ScriptableEquippable : ScriptableObject
{
    public string ItemName;
    [TextArea(3, 10)]
    public string ItemDesc;
    public Sprite ItemIcon;
    public string ItemResourceID;
}
