using System.Collections;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    public CREW Crew;
    public SpriteRenderer Loon;
    float CurrentActivation = 1f;
    public float ActivationSpeed = 1.1f;
    public float MinSize = 0.2f;
    public float MaxSize = 1f;

    private void OnEnable()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        bool Active = true;
        float Timer = 0f;
        while (true)
        {
            Timer -= CO.co.GetWorldSpeedDelta();
            if (Timer <= 0f)
            {
                Timer += 0.25f;
                if (!Crew.isDead()) Active = Crew.Space == null;
            }
            if (Active)
            {
                CurrentActivation += CO.co.GetWorldSpeedDelta() * ActivationSpeed;
               
            }
            else
            {
                CurrentActivation -= CO.co.GetWorldSpeedDelta() * ActivationSpeed * 2f;
            }
            Loon.transform.localScale = Vector3.one * Mathf.Lerp(MinSize, MaxSize, CurrentActivation);
            Loon.color = new Color(1, 1, 1, Mathf.Clamp01(CurrentActivation * 0.8f - 0.1f));
        }
    }
}
