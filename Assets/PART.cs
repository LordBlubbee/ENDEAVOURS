using System.Collections.Generic;
using UnityEngine;

public class PART : MonoBehaviour
{
    public List<ParticleSystem> AttachedParticles;
    private SpriteRenderer spr;
    public float FullFadeDuration;
    public float FadeChange;
    public float ScaleChange;
    float ShakeTotal;
    float Fade;
    float Scale;
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        Fade = spr.color.a;
        Scale = transform.localScale.x;
        foreach (ParticleSystem particle in AttachedParticles) particle.transform.SetParent(transform.parent);
    }
    void Update()
    {
        //FADE
        if (FullFadeDuration > 0f)
        {
            FullFadeDuration -= CO.co.GetWorldSpeedDelta();
        } else
        {
            Fade += FadeChange * CO.co.GetWorldSpeedDelta();
            spr.color = new Color(1, 1, 1, Fade);
            if (Fade < 0)
            {
                Destroy(gameObject);
                return;
            }
        }
            
        //SCALE
        Scale += ScaleChange * CO.co.GetWorldSpeedDelta();
        transform.localScale = new Vector3(Scale, Scale, 1);
        if (Scale < 0)
        {
            Destroy(gameObject);
            return;
        }
    }
}
