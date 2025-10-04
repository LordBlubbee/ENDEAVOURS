using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUD : MonoBehaviour
{
    public AudioSource src;
    //
    public void PlayAUD(AudioClip clip, float pitchshift = 0f)
    {
        int Maximum = 0;
        foreach (AUD aud in AUDCO.aud.GetActiveAudio())
        {
            if (aud.src.clip == clip)
            {
                Maximum++;
                if (Maximum > 3)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
        StartCoroutine(FinishAudio());
        src.clip = clip;
        src.pitch = 1f + Random.Range(-pitchshift, pitchshift);
        src.Play();
    }

    private bool isRemoved = false;
    IEnumerator FinishAudio()
    {
        AUDCO.aud.AddActiveAudio(this);
        yield return new WaitForSeconds(0.25f);
        if (!isRemoved)
        {
            isRemoved = true;
            AUDCO.aud.RemoveActiveAudio(this);
        }
    }

    private void Update()
    {
        if (!src.isPlaying)
        {
            if (!isRemoved)
            {
                isRemoved = true;
                AUDCO.aud.RemoveActiveAudio(this);
            }
            Destroy(gameObject);
        }
    }
}
