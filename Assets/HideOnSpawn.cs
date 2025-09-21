using UnityEngine;

public class HideOnSpawn : MonoBehaviour
{
    public SpriteRenderer spr;
    void Start()
    {
        spr.enabled = false;
    }
}
