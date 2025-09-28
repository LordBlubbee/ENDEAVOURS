using System;
using UnityEngine;

public class WalkableTile : MonoBehaviour
{
    [NonSerialized] public SPACE Space;
    public SpriteRenderer spr;
    public bool canBeBoarded = false;

    private void Start()
    {
        if (canBeBoarded) spr.color = new Color(1,1,0.7f);
    }
}
