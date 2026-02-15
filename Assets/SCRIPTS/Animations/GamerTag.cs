using TMPro;
using UnityEngine;

public class GamerTag : MonoBehaviour
{
    public TextMeshPro Name;
    public TextMeshPro Health;
    public SpriteRenderer FarIcon;
    public TextMeshPro FarHealth;
    private iDamageable FollowObject;
    private CREW Crew;
    private Module Mod;
    private ModuleArmor Armor;
    private GameObject FollowObjectRef;
    private bool UseFarIcon;
    public void SetPlayerAndName(CREW trans, string str, Color col)
    {
        //
        
        if (trans.IsPlayer())
        {
            Name.text = str;
            Name.color = col;
        }
        if (Crew) return;

        Crew = trans;
        FollowObject = trans;
        FollowObjectRef = trans.transform.gameObject;
        UseFarIcon = false;
        Destroy(FarIcon.gameObject);
        Destroy(FarHealth.gameObject);
    }

    private bool DamagedOnly = false;
    public void SetDamagedOnly()
    {
        DamagedOnly = true;
        UseFarIcon = false;
    }
    public void SetModuleObject(Module trans)
    {
        //
        Mod = trans;
        FollowObject = trans;
        FollowObjectRef = trans.transform.gameObject;
        UseFarIcon = true;
        if (Mod.ModuleType == Module.ModuleTypes.ARMOR)
        {
            Armor = (ModuleArmor)Mod;
        }
    }
    public void SetFarIcon(Sprite spr)
    {
        FarIcon.sprite = spr;
    }
    private void Update()
    {
        if (!FollowObjectRef)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = FollowObject.transform.position + new Vector3(0, 2);
        float healthRelative = FollowObject.GetHealthRelative();
        Color col = new Color(1 - healthRelative, healthRelative, 0);

        if (Crew)
        {
            if (Crew.isDeadForever())
            {
                if (Crew.isDeadButReviving())
                {
                    Health.text = $"REVIVING";
                    Health.color = new Color(0.5f, 0, 0.5f);
                } else
                {
                    Health.text = $"DEAD";
                    Health.color = new Color(0.5f, 0, 0);
                }
                return;
            }
            if (Crew.isDead())
            {
                if (Crew.BleedingTime.Value == 0) Health.text = $"BLEEDING";
                else Health.text = $"BLEEDING: {Crew.BleedingTime.Value.ToString("0")}";
                Health.color = new Color(1, 0, 0);
                return;
            }
            switch (Crew.GetTagState())
            {
                case CREW.TagStates.DORMANT:
                    Health.text = $"ZZZZZ";
                    Health.color = new Color(0.7f, 0, 0.8f);
                    return;
            }
            Health.text = $"{FollowObject.GetHealth().ToString("0")}/{FollowObject.GetMaxHealth().ToString("0")}";
            Health.color = col;
            return;
        }
        if (Mod)
        {
            if (FollowObject.GetHealthRelative() >= 1 && DamagedOnly)
            {
                Health.text = "";
                return;
            }
            else if (Mod.PermanentlyDead.Value)
            {
                Health.text = $"DESTROYED";
                Health.color = new Color(0.5f, 0, 0);
            }
            else if (Mod.IsDisabled())
            {
                Health.text = $"DISABLED \n<color=yellow>REPAIRS: {FollowObject.GetHealth().ToString("0")}%";
                Health.color = new Color(1, 0, 0);
            } else
            {
                Health.text = $"{FollowObject.GetHealth().ToString("0")}/{FollowObject.GetMaxHealth().ToString("0")}";
                Health.color = col;
            }

            if (UseFarIcon)
            {
                float far = CAM.cam.camob.orthographicSize;
                float scale = 0.5f + far * 0.01f;
                if (Mod.IsDisabled()) FarIcon.color = new Color(0.5f, 0, 0, Mathf.Clamp01(far * 0.04f - 2.4f));
                else FarIcon.color = new Color(col.r, col.g, col.b, Mathf.Clamp01(far * 0.04f - 2.4f));
                FarIcon.transform.localScale = new Vector3(scale, scale, 1);
                if (Armor)
                {
                    if (!Armor.CanAbsorbArmor())
                    {
                        FarHealth.text = "X";
                        FarHealth.color = new Color(1,0, 0, Mathf.Clamp01(far * 0.04f - 2.2f));
                    }
                    else
                    {
                        FarHealth.text = $"{Armor.GetArmor().ToString("0")}";
                        FarHealth.color = new Color(1,1,0, Mathf.Clamp01(far * 0.04f - 2.2f));
                    }
                    FarHealth.transform.localScale = new Vector3(scale, scale, 1);
                }
            }
        }
       
    }
}
