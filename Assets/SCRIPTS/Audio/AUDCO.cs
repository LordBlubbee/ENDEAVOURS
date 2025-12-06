using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AUDCO : MonoBehaviour
{
    public AudioSource OST1;
    public AudioSource OST2;
    protected AudioSource mainOST;
    protected AudioSource otherOST;
    public AUD spawnSFX;
    public static AUDCO aud;
    public enum BlockSoundEffects
    {
        NONE,
        SHIELD
    }

    public enum Soundtrack
    {
        NONE = -1,
        WASTES = 0, 
        NEBULOID = 1,
        EERIE = 2,
        RESPITE = 3,
        PRAGMATICUS = 4,
        INVICTUS = 5,
        STELLAE = 6,
        COALITION = 7,
        NOMADEN = 8,
        BAKUTO = 9,
        EPHEMERAL = 10,
        ROYALISTS = 11,
        DEMOCRATS = 12,
        SEEKERS = 13
    }

    public AudioClip[] SoundtrackCalm;
    public AudioClip[] SoundtrackIntense;
   

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
        mainOST = OST1;
        otherOST = OST2;
        StartCoroutine(SoundManager());
    }
    private void Update()
    {
    }
    IEnumerator SoundManager()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            if (CO.co)
            {
                switch (CO.co.CurrentSoundtrackID.Value)
                {
                    case -1:
                        setOST(null);
                        break;
                    default:
                        setOST(CO.co.IsSafe() ? SoundtrackCalm[CO.co.CurrentSoundtrackID.Value] : SoundtrackIntense[CO.co.CurrentSoundtrackID.Value]);
                        break;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
    public void setOST(AudioClip clip)
    {
        if (mainOST.clip == clip) return;
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
        mainOST.Stop();
       StartCoroutine(regulateOSTDir());
    }
    bool isRegulatingOSTDir = false;
    IEnumerator regulateOSTDir()
    {
        while (isRegulatingOSTDir) yield return null;
        isRegulatingOSTDir = true;
        mainOST.volume = 0f;
        while (otherOST.volume > 0f)
        {
            otherOST.volume -= Time.deltaTime * 0.5f;
            yield return null;
        }
        otherOST.volume = 0f;
        /*while (mainOST.volume < 1f)
        {
            mainOST.volume += Time.deltaTime * 0.5f; 
            otherOST.volume = 0f;
            yield return null;
        }*/
        mainOST.volume = 1f;
        otherOST.volume = 0f;
        otherOST.Stop();
        mainOST.Play();
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
        AUD aud = Instantiate(spawnSFX, trt, Quaternion.identity);
        aud.transform.SetParent(CAM.cam.transform);
        float Dist = (CAM.cam.transform.position - trt).magnitude;
        //Debug.Log($"Sound effect {clip.name} was {Dist} away resulting in sound level {Mathf.Clamp01((70f - Dist) * 0.04f)}");
        aud.PlayAUD(clip, pitchshift, Mathf.Clamp01((70f-Dist)*0.04f));
    }
    public void PlaySFX(AudioClip[] clips, Vector3 trt, float pitchshift = 0f)
    {
        PlaySFX(clips[Random.Range(0, clips.Length)], trt, pitchshift);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayBlockSFXRpc(BlockSoundEffects eff, Vector3 trt)
    {
        PlayBlockSFX(eff, trt);
    }
    public void PlayBlockSFX(BlockSoundEffects eff, Vector3 trt)
    {
        switch (eff)
        {
            default:
                PlaySFX(Block_Shield, trt, 0.1f);
                break;
        }
    }

    [Header("SFX Clips")]
    public AudioClip[] Block_Shield;
    public AudioClip Upgrade;
    public AudioClip Purchase;
    public AudioClip EquipItem;
    public AudioClip Salvage;
    public AudioClip Press;
    public AudioClip Fail;
}
