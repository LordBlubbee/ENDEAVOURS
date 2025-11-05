using System.Collections;
using UnityEngine;

public class Darkness : MonoBehaviour
{
    public SpriteRenderer spr;
    float Fade = 1f;
    private float TargetFade = 1f;
    void Start()
    {
        spr.enabled = true;
        StartCoroutine(UpdateDarkness());
    }
    IEnumerator UpdateDarkness()
    {
        while (!LOCALCO.local.GetPlayer()) yield return null;
        while (true)
        {
            CREW play = LOCALCO.local.GetPlayer();
            if (play.HasLineOfSight(transform.position)) TargetFade = (play.transform.position-transform.position).magnitude*0.06f-0.6f;
            else TargetFade = 1f;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Update()
    {
        Fade = Mathf.Lerp(Fade, TargetFade, 2f * CO.co.GetWorldSpeedDelta());
        spr.color = new Color(0, 0, 0, Fade);
    }
}
