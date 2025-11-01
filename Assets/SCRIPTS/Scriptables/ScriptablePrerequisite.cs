using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptablePrerequisite : ScriptableObject
{
    public abstract bool IsTrue();
}
