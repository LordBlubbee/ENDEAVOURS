using System.Collections.Generic;
using UnityEngine;

public class PART : MonoBehaviour
{
    public List<ParticleSystem> AttachedParticles;
    private SpriteRenderer spr;
    public float FullFadeDuration;
    public float FadeChange;
    public float ScaleChange;
    public float RandomMovement = 0;
    Vector3 Movement = Vector3.zero;
    float MoveFactor = 1f;
    float Fade;
    float Scale;
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        Fade = spr.color.a;
        Scale = transform.localScale.x;
        if (RandomMovement > 0)
        {
            Movement = new Vector3(Random.Range(-RandomMovement, RandomMovement), Random.Range(-RandomMovement, RandomMovement), 0);
        }
        foreach (ParticleSystem particle in AttachedParticles) particle.transform.SetParent(transform.parent);
    }
    void Update()
    {
        //FADE
        if (Movement != Vector3.zero)
        {
            MoveFactor = Mathf.Max(MoveFactor-FadeChange * 0.7f * CO.co.GetWorldSpeedDelta(), 0);
            transform.position += Movement * MoveFactor * CO.co.GetWorldSpeedDelta();
        }
        if (FullFadeDuration > 0f)
        {
            FullFadeDuration -= CO.co.GetWorldSpeedDelta();
        } else
        {
            Fade += FadeChange * CO.co.GetWorldSpeedDelta();
            spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, Fade);
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
