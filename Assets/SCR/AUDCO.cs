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
        Instantiate(spawnSFX, CAM.cam.transform.position, Quaternion.identity).PlayAUD(clip, pitchshift);
    }
    public void PlaySFX(AudioClip[] clips, float pitchshift = 0f)
    {
        PlaySFX(clips[Random.Range(0, clips.Length)], pitchshift);
    }
    public void PlaySFX(AudioClip clip, Vector3 trt, float pitchshift = 0f)
    {
        Instantiate(spawnSFX, trt, Quaternion.identity).PlayAUD(clip, pitchshift);
    }
    public void PlaySFX(AudioClip[] clips, Vector3 trt, float pitchshift = 0f)
    {
        PlaySFX(clips[Random.Range(0, clips.Length)], trt, pitchshift);
    }
    public void SetRandomRestOST()
    {
        SetRandomRestOSTClientRpc(Random.Range(0, OST_Rest.Length));
    }
    [ClientRpc]
    public void SetRandomRestOSTClientRpc(int ID)
    {
        setOST(OST_Rest[ID]);
    }
    public void SetRandomOST()
    {
        SetRandomOSTClientRpc(Random.Range(0, OST_Preparation.Length));
    }
    [ClientRpc]
    public void SetRandomOSTClientRpc(int ID)
    {
        SetOSTBattle(ID);
    }
    public void SetOSTBattle(int ID)
    {
        StartCoroutine(BattleOSTWait(ID));
    }
    IEnumerator BattleOSTWait(int ID)
    {
        setOST(OST_Preparation[ID]);
        yield return new WaitForSeconds(mainOST.clip.length);
        if (mainOST.clip == OST_Preparation[ID])
        {
            setOST(OST_Intense[ID]);
        }
    }

    [Header("OST Clips")]
    public AudioClip[] OST_Rest;
    public AudioClip[] OST_Preparation;
    public AudioClip[] OST_Intense;

    [Header("SFX Clips")]
    public AudioClip Press;
}
