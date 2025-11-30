using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.UI;

public class UI_Buff : MonoBehaviour
{
    public TextMeshProUGUI StackCount;
    public Image Cooldown;
    public Image BuffIcon;
    public TooltipObject BuffDesc;
    float MaxCooldown;
    float Cooldownleft;
    float LastUpdate;
    ParticleBuff CurrentBuff;

    public ParticleBuff GetCurrentBuff()
    {
        return CurrentBuff;
    }
    public void SetBuff(ParticleBuff buff, int Stacks)
    {
        CurrentBuff = buff;
        if (buff == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        if (Stacks < 2) StackCount.text = "";
        else StackCount.text = Stacks.ToString();
        MaxCooldown = buff.ExpectedDuration;
        Cooldownleft = buff.ExpectedDuration;
        Cooldown.color = buff.BuffColor;
        Cooldown.fillAmount = 1;
        BuffIcon.sprite = buff.BuffIcon;
        BuffDesc.Tooltip = buff.BuffDesc;
    }
    private void Update()
    {
        if (MaxCooldown < 0) return;
        Cooldownleft -= LastUpdate - Time.time;
        Cooldown.fillAmount = Cooldownleft/MaxCooldown;
        LastUpdate = Time.time;
    }
}
