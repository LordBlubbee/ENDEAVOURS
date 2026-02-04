using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Screen_Settings : MonoBehaviour
{
    public Slider VCX_Vol;
    public Slider SFX_Vol;
    public Slider OST_Vol;
    public TextMeshProUGUI VCX_Tex;
    public TextMeshProUGUI SFX_Tex;
    public TextMeshProUGUI OST_Tex;
    public TextMeshProUGUI Tutorial_Tex;
    public AudioMixer AudioMixer;

    public void Init()
    {
        VCX_Vol.value = GO.g.VCX_Vol;
        SFX_Vol.value = GO.g.SFX_Vol;
        OST_Vol.value = GO.g.OST_Vol;
        Tutorial_Tex.color = GO.g.enableTutorial ? Color.yellow : Color.gray;
        Refresh();
    }
    private void Refresh()
    {
        VCX_Tex.text = $"Volume Voices\n{(VCX_Vol.value * 20f - 10f).ToString("0.0")}dB";
        SFX_Tex.text = $"Volume Sound Effects\n{(SFX_Vol.value * 20f - 10f).ToString("0.0")}dB";
        OST_Tex.text = $"Volume Soundtrack\n{(OST_Vol.value * 20f - 10f).ToString("0.0")}dB";
    }

    public void PressTutorial()
    {
        GO.g.enableTutorial = !GO.g.enableTutorial;
        GO.g.saveSettings();
        Tutorial_Tex.color = GO.g.enableTutorial ? Color.yellow : Color.gray;
    }
    public void ResetGame()
    {
        gameObject.SetActive(false);
    }
    public void RedoTutorial()
    {
    }

    public void AdjustingVolumeVCX()
    {
        if (VCX_Vol.value == 0) AudioMixer.SetFloat("VCXVol", -99f);
        else AudioMixer.SetFloat("VCXVol", VCX_Vol.value*20f-10f);
        GO.g.VCX_Vol = VCX_Vol.value;
        GO.g.saveSettings();
        Refresh();
    }
    public void AdjustingVolumeSFX()
    {
        if (SFX_Vol.value == 0) AudioMixer.SetFloat("SFXVol", -99f);
        else AudioMixer.SetFloat("SFXVol", SFX_Vol.value * 20f - 12f);
        GO.g.SFX_Vol = SFX_Vol.value;
        GO.g.saveSettings();
        Refresh();
    }
    public void AdjustingVolumeOST()
    {
        if (OST_Vol.value == 0) AudioMixer.SetFloat("OSTVol", -99f);
        else AudioMixer.SetFloat("OSTVol", OST_Vol.value * 20f - 20f);
        GO.g.OST_Vol = OST_Vol.value;
        GO.g.saveSettings();
        Refresh();
    }

    public void pressQuit()
    {
        Application.Quit();
    }

    public void pressFullSCreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
