using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AUDCO : NetworkBehaviour
{
    public AudioSource OST1;
    public AudioSource OST2;
    protected AudioSource mainOST;
    protected AudioSource otherOST;
    public AUD spawnSFX;
    public static AUDCO aud;

    private List<AUD> ActiveAudio = new();

    public List<AUD> GetActiveAudio()
    {
        return ActiveAudio;
    }

    public void AddActiveAudio(AUD aud)
    {
        ActiveAudio.Add(aud);
    }
    public void RemoveActiveAudio(AUD aud)
    {
        ActiveAudio.Remove(aud);
    }
    private void Start()
    {
        aud = this;
    }
    public void setOST(AudioClip clip)
    {
        if (mainOST == OST1)
        {
            otherOST = OST1;
            mainOST = OST2;
        }
        else
        {
            otherOST = OST2;
            mainOST = OST1;
        }
        mainOST.clip = clip;
        mainOST.Play();
        if (!isRegulatingOSTDir) StartCoroutine(regulateOSTDir());
    }
    bool isRegulatingOSTDir = false;
    IEnumerator regulateOSTDir()
    {
        isRegulatingOSTDir = true;
        while (mainOST.volume < 1f)
        {
            mainOST.volume += Time.deltaTime * 0.5f;
            otherOST.volume = 1f - mainOST.volume;
            yield return null;
        }
        mainOST.volume = 1f;
        otherOST.volume = 0f;
        isRegulatingOSTDir = false;
    }
    public void PlaySFX(AudioClip clip, float pitchshift = 0f)
    {
        Instantiate(spawnSFX, CAM.cam.transform).PlayAUD(clip, pitchshift);
    }
    public void PlaySFX(AudioClip[] clips, float pitchshift = 0f)
    {
        PlaySFX(clips[Random.Range(0, clips.Length)], pitchshift);
    }
    public void PlaySFX(AudioClip clip, Vector3 trt, float pitchshift = 0f)
    {
        trt = new Vector3(trt.x,trt.y,CAM.cam.transform.position.z);
        Instantiate(spawnSFX, trt, Quaternion.identity).PlayAUD(clip, pitchshift);
    }
    public void PlaySFX(AudioClip[] clips, Vector3 trt, float pitchshift = 0f)
    {
        PlaySFX(clips[Random.Range(0, clips.Length)], trt, pitchshift);
    }

    [Header("SFX Clips")]
    public AudioClip Press;
    public AudioClip Fail;
}
